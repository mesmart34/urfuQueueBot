using System.Collections.Generic;
using TableParser;

namespace TableQueries
{
    // TODO: Добавить реализацию IQuery

    public interface IQuery
    {
        IEnumerable<Room> ParseRoom(string tableId);
        IEnumerable<Team> GetTeamsByRoomId(string tableId, string roomId);
        void AddMemberToTeam(Room room, Team team, string member);
        void RemoveMemberFromTeam(Room room, Team team, string member);
    }
}
