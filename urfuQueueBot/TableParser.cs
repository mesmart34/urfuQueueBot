using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;

namespace urfuQueueBot
{
    class TableParser
    {
        private GoogleCredential credential;
        private SheetsService service;
        private string spreadSheetId;
        private string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private readonly string[] keywords = new string[] { "Эксперты", "Модераторы", "Комната", "Защита" };
        private enum ReadMode
        {
            None, ExpertMode, ModeratorMode, TeamMode, TimeMode
        }


        public TableParser(string spreadSheet)
        {
            spreadSheetId = spreadSheet;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "urfuQueueBot"
            });
        }

        public List<Room> Parse()
        {
            var sheets = GetAllSheets();
            var rooms = new List<Room>();
            foreach (var sheet in sheets)
            {
                var table = Read(sheet);
                ProcessTable(table, 0);
                ProcessTable(table, 1);
            }
            return rooms;
        }

        private bool IsKeyword(string value)
        {
            foreach (var v in keywords)
            {
                if (v == value)
                    return true;
            }
            return false;
        }

        public Room ProcessTable(IList<IList<object>> data, int offset)
        {
            var mode = ReadMode.None;
            var room = new Room();
            DateTime time = new DateTime();
            foreach (var row in data)
            {
                var value = row[offset].ToString();
                //if (IsKeyword(value))
                if (value.StartsWith("Эксперты"))
                    mode = ReadMode.ExpertMode;
                else if (value.StartsWith("Модераторы"))
                    mode = ReadMode.ModeratorMode;
                else if (value.StartsWith("Защита"))
                {
                    mode = ReadMode.TimeMode;
                    var splited = value.Split(' ');
                    time = Convert.ToDateTime(splited[3]);
                    room.teams.Add(time, new List<Team>());
                }
                else if (value.StartsWith("Комната"))
                {
                    mode = ReadMode.TeamMode;
                    room.name = value;
                }
                else
                {
                    switch (mode)
                    {
                        case ReadMode.ExpertMode:
                            {
                                room.experts.Add(new Expert(value));
                            }
                            break;
                        case ReadMode.ModeratorMode:
                            {
                                room.moderators.Add(new Moderator(value));
                            }
                            break;
                        case ReadMode.TeamMode:
                            {
                                var team = new Team();
                                team.Name = value;
                                room.teams[time].Add(team);
                            }
                            break;
                    };
                }
            }
            return room;
        }

        public List<string> GetAllSheets()
        {
            var request = service.Spreadsheets.Get(spreadSheetId);
            var response = request.Execute();
            var sheetList = new List<string>();
            foreach (var sheet in response.Sheets)
            {
                sheetList.Add(sheet.Properties.Title);
            }
            return sheetList;
        }

        public IList<IList<object>> Read(string sheet)
        {
            var request = service.Spreadsheets.Values.Get(spreadSheetId, sheet);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
                return values;
            return null;
        }

        public IList<IList<object>> Read(string sheet, string range)
        {
            var request = service.Spreadsheets.Values.Get(spreadSheetId, sheet + "!" + range);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
                return values;
            return null;
        }

        public void Write(string sheet, string range, List<IList<object>> values)
        {
            var valueRange = new ValueRange();
            var request = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, sheet + "!" + range);
            request.Execute();
        }
    }
}
