using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class DataBase
    {
        private TableIO table;

        public DataBase(string spriteSheetId)
        {
            table = new TableIO(spriteSheetId);
        }

        //Update google sheet with teams
        public void UpdateWhole(Dictionary<string, Room> rooms)
        {
            var sheets = table.GetAllSheets();
            foreach (var sheet in sheets)
            {
                if (sheet.Name != "Sheet1")
                    table.DeleteSheet(sheet.Id);
            }
            foreach (var room in rooms.Values)
            {
                UpdateRoom(room);
            }
        }

        public void UpdateRoom(Room room)
        {
            var data = new List<IList<object>>();
            var timeKeys = room.Teams.Keys.ToList();
            for (var j = 0; j < timeKeys.Count; j++)
            {
                var timeKey = timeKeys[j];
                foreach (var team in room.Teams[timeKey])
                {
                    var column = new List<object>();
                    column.Add(timeKey.ToString());
                    column.Add(team.Name + "," + team.Time.ToString());
                    data.Add(column);
                    for (var s = 0; s < Math.Max(team.Students.Count, 10); s++) //ДИКИЙ КОСТЫЛЬ
                    {
                        if (s < team.Students.Count)
                            column.Add(s);
                        else 
                            column.Add("");
                    }
                }
            }
            data = GetTransposed(data);
            var link = room.GetLink();
            table.CreateSheet(link);
            table.Write(link, data);
            //table.Write(link, GetTransposed(data));
        }

        private List<IList<object>> GetTransposed(List<IList<object>> data)
        {
            var result = new List<IList<object>>();
            for (var i = 0; i < data[0].Count; i++)
            {
                var line = new List<object>();
                result.Add(line);
                for (var j = 0; j < data.Count; j++)
                    line.Add(0);
            }
            for (int x = 0; x < data.Count; ++x) //Iterate through the horizontal rows of the two dimensional array
            {
                for (int y = 0; y < data[0].Count; ++y) //Iterate throught the vertical rows, to add more dimensions add another for loop for z
                {
                    result[y][x] = data[x][y]; //Change result x,y to input x,y
                }
            }
            return result;
        }

        //Fills rooms with teams from google sheet
        public void Read(Dictionary<string, Room> rooms)
        {
            foreach (var sheet in table.GetAllSheets())
            {
                if (sheet.Name == "Sheet1")
                    continue;
                var link = sheet.Name;
                if (!rooms.ContainsKey(link))
                    continue;
                var room = rooms[link];
                var data = table.Read(link);
                var timeKeys = new List<DateTime>();
                for (var row = 0; row < data.Count; row++)
                {
                    for (var col = 0; col < data[row].Count; col++)
                    {
                        var value = (string)data[row][col];
                        if (value.Length == 0)
                            continue;
                        if (row == 0)
                        {
                            var roomTime = DateTime.Parse(value);
                            timeKeys.Add(roomTime);
                        }
                        else if (row == 1)
                        {
                            var titleData = value.Split(',');
                            var time = DateTime.Parse(titleData[1]);
                            room.Teams[timeKeys[col]][row].ChangeTime(time);
                        }
                        else
                        {
                            var key = timeKeys[col];
                            var team = room.Teams[key][col];
                            team.AddStudent(value);
                        }
                    }
                }
            }
        }
    }
}
