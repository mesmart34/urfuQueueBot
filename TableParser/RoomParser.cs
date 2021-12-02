﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public static class RoomParser
    {
        public static Room CreateRoom(TableIO table, SheetData sheet)
        {
            var id = table.GetID();
            var roomData = table.Read(sheet.Name);
            var room = GetRoom(roomData, id);
            return room;
        }

        private static Room GetRoom(IList<IList<object>> data, string tableID)
        {
            var teams = new Dictionary<DateTime, List<Team>>();
            GetTeams(teams, data, 0);
            GetTeams(teams, data, 1);
            var roomNameId = GetRoomCellIdByContent(data, "Защита", 0) + 1;
            var roomName = (string)data[roomNameId][0];
            return new Room(roomName, tableID, teams);
        }

        private static int GetRoomCellIdByContent(IList<IList<object>> table, string content, int offset)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var value = (string)table[i][offset];
                if (value.StartsWith(content))
                    return i;
            }
            return -1;
        }

        private static void GetTeams(Dictionary<DateTime, List<Team>> teams, IList<IList<object>> data, int offset)
        {
            var from = GetRoomCellIdByContent(data, "Защита", offset);
            var roomData = (string)(data[from][offset]);
            var roomDataSplited = roomData.Split(' ');
            var timeStr = roomDataSplited[1] + " " + roomDataSplited[3];
            var time = DateTime.ParseExact(timeStr, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            for (var i = from + 2; i < data.Count; i++)
            {
                var value = data[i][offset].ToString();
                if (!teams.ContainsKey(time))
                {
                    teams[time] = new List<Team>();
                }
                var team = new Team(value, time);
                teams[time].Add(team);
            }
        }
    }
}
