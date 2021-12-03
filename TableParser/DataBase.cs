﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TableParser
{
    public class DataBase
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
                    for (var s = 0; s < team.Students.Count; s++)
                    {
                        column.Add(team.Students[s]);

                    }
                }
            }
            var link = room.GetLink();
            table.CreateSheet(link);
            table.Write(link, data);
        }

        public void Read(Room room)
        {
            var data = table.Read(room.GetLink());
            foreach (var row in data)
            {
                var time = DateTime.Parse((string)row[0]);
                var value = (string)row[1];
                var title = value.Split(',');
                var teamCounter = 0;
                var team = new Team(title[0], teamCounter++, DateTime.Parse(title[1]));
                room.AddTeam(team, time);
                for (var i = 2; i < row.Count; i++)
                {
                    value = (string)row[i];
                    if (value.Length == 0)
                        continue;
                    team.AddStudent(value);
                }
            }
        }

        //Fills rooms with teams from google sheet
        public void ReadWhole(Dictionary<string, Room> rooms)
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
                var teamCounter = 0;
                foreach (var row in data)
                {
                    var time = DateTime.Parse((string)row[0]);
                    var value = (string)row[1];
                    var title = value.Split(',');
                    var team = new Team(title[0], teamCounter++, DateTime.Parse(title[1]));
                    room.AddTeam(team, time);
                    for (var i = 2; i < row.Count; i++)
                    {
                        value = (string)row[i];
                        if (value.Length == 0)
                            continue;
                        team.AddStudent(value);
                    }
                }
            }
        }
    }
}
