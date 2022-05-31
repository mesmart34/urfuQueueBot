using System.Collections.Generic;
using TableParser;

namespace TableParser
{
    public static class Query
    {
        public static void AddMemberToTeam(this DataBase db, Room room, Team team, IMember member)
        {
            team.AddStudent(member);
            db.WriteRoom(room);
        }

        public static IEnumerable<Room> ParseRoom(this DataBase db, string tableId)
        {
            var roomsTable = new TableIO(tableId);
            foreach (var sheet in roomsTable.GetAllSheets())
            {
                var rooms = roomsTable.ParseRooms(sheet);
                foreach (var room in rooms)
                {
                    db.ParseRoom(room);
                    yield return room;
                }
            }
        }

        public static IEnumerable<string> GetTeamsNamesByRoomId(this DataBase db, string roomId)
        {
            return db.GetTeamsNames(roomId);
        }

        public static void RemoveMemberFromTeam(this DataBase db, Room room, Team team, IMember member)
        {
            team.RemoveStudent(member);
            db.WriteRoom(room);
        }

        public static int GetTeamIndex(TableIO table, Room room, string teamName)
        {
            var data = table.Read(room.Name);
            for (int i = 0; i < data[0].Count; ++i)
            {
                var startColumn = TableIO.GetRoomCellIdByContent(data, room.Name, i) + 1;
                for (int c = startColumn; c < data.Count; ++c)
                {
                    if ((string)data[c][i] == teamName)
                        return c;
                }
            }
            return -1;
        }
    }
}
