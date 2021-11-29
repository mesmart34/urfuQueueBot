using System;
using System.Collections.Generic;
using TableParser;

namespace TableQueries
{
    // TODO: Добавить реализацию IQuery

    public interface IQuery
    {
        IEnumerable<Room> GetRooms(string tableId);
        IEnumerable<Team> GetTeamsByRoomId(string roomId);
        void AddMemberToTeam(string member);
        void RemoveMemberFromTeam(string member);
    }
}
