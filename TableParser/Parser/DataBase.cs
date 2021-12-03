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
            foreach (var sheet in sheets)
            {
                if (sheet.Name != "Sheet1")
                    _table.DeleteSheet(sheet.Id);
            }
            foreach (var room in rooms)
            {
                WriteRoom(room);
            }
        }

        public void WriteRoom(Room room)
        {
            var data = new List<IList<object>>();
            //var timeKeys = room.Teams.Keys.ToList();
            var time = room.StartTime;
            // TEAM_NAME | TIME | MEMBERS
            foreach (var team in room.Teams)
            {
                var column = new List<object>();
                column.Add(team.Name);
                column.Add(time.ToString());
                foreach (var student in team.Members)
                {
                    column.Add(student);
                }
            }

            var link = room.Link;
            _table.CreateSheet(link);
            _table.Write(link, data);
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
            foreach (var sheet in _table.GetAllSheets())
            {
                if (sheet.Name == "Sheet1")
                    continue;

                var room = links[sheet.Name];
                Read(room);
            }
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
