using System.Collections.Generic;
using TableParser;

namespace TableQueries
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
                var rooms = RoomParser.CreateRooms(roomsTable, sheet);
                foreach (var room in rooms)
                {
                    db.Read(room);
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
            var data = table.Read(room.Link);
            for (int i = 0; i < data.Count; ++i)
            {
                var startRow = RoomParser.GetRoomCellIdByContent(data, room.Name, i) + 1;
                for (int r = 0; r < data[i].Count; ++r)
                {
                    if ((string)data[i][r + startRow] == teamName)
                        return r;
                }
            }
            return -1;
        }
    }
}
