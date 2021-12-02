using System.Collections.Generic;
using TableParser;

namespace TableQueries
{
    public class Query : IQuery
    {
        private DataBase _data;

        public Query(DataBase dataBase)
        {
            _data = dataBase;
        }

        public void AddMemberToTeam(Room room, Team team, string member)
        {
            team.AddStudent(member);
            _data.UpdateRoom(room);
        }

        public IEnumerable<Room> ParseRoom(string tableId)
        {
            var roomsTable = new TableIO(tableId);
            foreach (var sheet in roomsTable.GetAllSheets())
            {
                var room = RoomParser.CreateRoom(roomsTable, sheet);
                _data.Read(room);
                yield return room;
            }
        }

        public IEnumerable<Team> GetTeamsByRoomId(string tableId, string roomId)
        {
            var roomsTable = new TableIO(tableId);
            foreach (var sheet in roomsTable.GetAllSheets())
            {
                if (sheet.Name != roomId)
                    continue;
                var room = RoomParser.CreateRoom(roomsTable, sheet);
                _data.Read(room);
                foreach (var teams in room.Teams.Values)
                {
                    foreach (var team in teams)
                        yield return team;
                }
            }
        }

        public void RemoveMemberFromTeam(Room room, Team team, string member)
        {
            team.RemoveStudent(member);
            _data.UpdateRoom(room);
        }
    }
}
