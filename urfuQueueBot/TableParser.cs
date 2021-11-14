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
                var teams = new Dictionary<DateTime, List<Team>>();
                teams = GetTeams(teams, table, 0);
                teams = GetTeams(teams, table, 1);
                var room = new Room("Комната " + rooms.Count.ToString(), teams);
                rooms.Add(room);
            }
            return rooms;
        }

        private int GetRoomCellId(IList<IList<object>> table, int offset)
        {
            for(var i = 0; i < table.Count; i++)
            {
                var value = (string)table[i][offset];
                if (value.StartsWith("Защита"))
                    return i;
            }
            return -1;
        }

        public Dictionary<DateTime, List<Team>> GetTeams(Dictionary<DateTime, List<Team>> teams, IList<IList<object>> data, int offset)
        {
            var from = GetRoomCellId(data, offset);
            var roomData = (string)(data[from][offset]);
            var roomDataSplited = roomData.Split(' ');
            var timeStr = roomDataSplited[1] + " " + roomDataSplited[3];
            var time = DateTime.ParseExact(timeStr, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            for (var i = from + 2; i < data.Count; i++)
            {
                var value = data[i][offset].ToString();
                var team = new Team();
                team.Name = value;
                team.Time = time;
                if (!teams.ContainsKey(time))
                    teams[time] = new List<Team>();
                teams[time].Add(team);
            }
            return teams;
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
