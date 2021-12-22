using System;
using System.Collections.Generic;
using System.Linq;

namespace TableParser
{
    public class DataBase
    {
        private readonly TableIO _table;

        public DataBase(string spriteSheetId)
        {
            _table = new TableIO(spriteSheetId);
        }

        //Update google sheet with teams
        public void WriteWhole(List<Room> rooms)
        {
            var sheets = _table.GetAllSheets();
            foreach (var sheet in sheets.Skip(1))
            {
                _table.DeleteSheet(sheet.Id);
            }
            foreach (var room in rooms)
            {
                WriteRoom(room);
            }
        }

        public void WriteRoom(Room room)
        {
            if (_table.GetAllSheets().Select(sheet => sheet.Name).Contains(room.Link))
                _table.ClearSheet(room.Link);

            var data = new List<IList<object>>();
            var time = room.StartTime;
            // TEAM_NAME | TIME | MEMBERS
            foreach (var team in room.Teams)
            {
                var column = new List<object>();
                column.Add(team.Name);
                column.Add(team.Time.ToString());
                foreach (var student in team.Members)
                {
                    column.Add(student.Name + ":" + ((int)student.Notification).ToString());
                }
                data.Add(column);
            }

            var link = room.Link;
            _table.CreateSheet(link);
            _table.Write(link, data);


            _table.AppendRow("Linker", new List<object> { room.Link, room.TableID });
        }

        public void Read(Room room)
        {
            var data = _table.Read(room.Link);
            var teamCounter = 0;
            foreach (var row in data)
            {
                var name = (string)row[0];
                var time = DateTime.Parse((string)row[1]);
                var team = new Team(name, teamCounter++, time);
                room.AddTeam(team);
                for (var i = 2; i < row.Count; i++)
                {
                    name = (string)row[i];
                    if (name.Length == 0)
                        continue;
                    team.AddStudent(new Member(name));
                }
            }
        }

        //Fills rooms with teams from google sheet
        public void ReadWhole(List<Room> rooms)
        {
            var links = rooms.ToDictionary(room => room.Link);
            foreach (var sheet in _table.GetAllSheets().Skip(1))
            {
                var room = links[sheet.Name];
                Read(room);
            }
        }

        public IEnumerable<Room> GetRooms()
        {
            List<Room> res = new List<Room>();
            var sheets = _table.GetAllSheets();
            foreach (var sheet in sheets.Skip(1))
            {
                string[] sheetInfo = sheet.Name.Split(':');
                // TODO: throw own exception
                int roomNumber = int.Parse(new string(sheetInfo[0].ToCharArray().Where(c => char.IsDigit(c)).ToArray()));

                var data = _table.Read(sheet.Name);
                DateTime roomTime = DateTime.Parse((string)data[0][1]);
                List<Team> teams = new List<Team>();
                foreach (var row in data)
                {
                    string teamName = (string)row[0];
                    DateTime teamTime = DateTime.Parse((string)row[1]);
                    Team newTeam = new Team(teamName, teams.Count, teamTime);

                    foreach (var member in row.Skip(2))
                    {
                        string[] memberInfo = ((string)member).Split(':');
                        newTeam.AddStudent(new Member(memberInfo[0], (NotificationType)int.Parse(memberInfo[1])));
                    }

                    teams.Add(newTeam);
                }

                var linksData = _table.Read("Linker").Skip(1);
                string link = (string)linksData.Where(row => (string)row[0] == sheet.Name).First()[1];

                res.Add(new Room(roomNumber, link, teams, roomTime));
            }

            return res;
        }

        public IEnumerable<string> GetTeamsNames(string roomId)
        {
            List<string> res = new List<string>();
            var data = _table.Read(roomId);
            foreach (var row in data)
            {
                var name = (string)row[0];
                res.Add(name);
            }
            return res;
        }
    }
}
