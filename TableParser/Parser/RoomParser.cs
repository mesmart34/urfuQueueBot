﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public static class RoomParser
    {
        public static IEnumerable<Room> CreateRooms(TableIO table, SheetData sheet)
        {
            var id = table.GetID();
            var roomData = table.Read(sheet.Name);
            var rooms = GetRooms(roomData, id);
            return rooms;
        }

        private static IEnumerable<Room> GetRooms(IList<IList<object>> data, string tableID)
        {
            var rooms = new List<Room>();
            rooms.Add(GetRoom(data, 0, tableID));
            rooms.Add(GetRoom(data, 1, tableID));
            return rooms;
        }

        public static int GetRoomCellIdByContent(IList<IList<object>> table, string content, int offset)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var value = (string)table[i][offset];
                if (value.StartsWith(content))
                    return i;
            }
            return -1;
        }

        private static Room GetRoom(IList<IList<object>> data, int offset, string tableID)
        {
            var teams = new List<Team>();
            var from = GetRoomCellIdByContent(data, "Защита", offset);
            var roomData = (string)(data[from][offset]);
            var roomDataSplited = roomData.Split(' ');
            var timeStr = roomDataSplited[1] + " " + roomDataSplited[3];
            var time = DateTime.ParseExact(timeStr, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            for (var i = from + 2; i < data.Count; i++)
            {
                var name = data[i][offset].ToString();
                var team = new Team(name, teams.Count, time);
                teams.Add(team);
            }

            var roomNameId = from + 1;
            var roomName = (string)data[roomNameId][offset];
            return new Room(roomName, tableID, teams, time);
        }
    }
}