using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableParser;

namespace SQLDB
{
    public class DataBase : IO
    {
        private Table _linker;

        public DataBase() : base("SQLEXPRESS", "DB")
        {
            if (!IsTableExists("Linker"))
                _linker = CreateTable("Linker");
            else
                _linker = Tables.First(t => t.Name == "Linker");

        }

        //public IEnumerable<Room> GetRooms()
        //{
        //    List<Room> res = new List<Room>();

        //    List<string> roomCodes = new List<string>();

        //    var tables = SendNativeQuery("SELECT TABLE_NAME AS [TABLE NAME] FROM INFORMATION_SCHEMA.TABLES").ExecuteReader();

        //    if (tables.HasRows)
        //    {
        //        while (tables.Read())
        //        {
        //            roomCodes.Add(tables.GetString(0));
        //        }
        //    }

        //    tables.Close();

        //    foreach (var roomCode in roomCodes.Skip(1))
        //    {
        //        // TODO: throw own exception
        //        int roomNumber = int.Parse(new string(roomCode.ToCharArray().TakeWhile(c => char.IsDigit(c)).ToArray()));

        //        var roomData = SendNativeQuery($"SELECT * FROM {roomCode}").ExecuteReader();

        //        DateTime roomTime = DateTime.MinValue;
        //        List<Team> teams = new List<Team>();

        //        if (roomData.HasRows)
        //        {
        //            while (roomData.Read())
        //            {
        //                if (roomTime == DateTime.MinValue)
        //                    roomTime = roomData.GetDateTime(1);

        //            }
        //        }


        //        foreach (var row in data)
        //        {
        //            string teamName = (string)row[0];
        //            DateTime teamTime = DateTime.Parse((string)row[1]);
        //            Team newTeam = new Team(teamName, teams.Count, teamTime);

        //            foreach (var member in row.Skip(2))
        //            {
        //                string[] memberInfo = ((string)member).Split(':');
        //                newTeam.AddStudent(new Member(memberInfo[0], (NotificationType)int.Parse(memberInfo[1])));
        //            }

        //            teams.Add(newTeam);
        //        }

        //        var linksData = Table.Read("Linker").Skip(1);
        //        string link = (string)linksData.Where(row => (string)row[0] == sheet.Name).First()[1];

        //        res.Add(new Room(roomNumber, link, teams, roomTime));
        //    }

        //    return res;
        //}

        public void WriteRoom(TableParser.Room room)
        {
            // delete room sheet
            if (IsTableExists(room.Link))
                SendNativeQuery($"TRUNCATE TABLE {room.Link}");

            var time = room.StartTime;
            Table roomTable = Tables.First(t => t.Name == room.Link);

            foreach (var team in room.Teams)
            {
                var teamRow = new List<object>();
                teamRow.Add(team.Name);
                teamRow.Add(team.Time.ToString());
                foreach (var student in team.Members)
                {
                    teamRow.Add(student.Name + ":" + ((int)student.Notification).ToString());
                }

                roomTable.Append(teamRow);
            }

            _linker.Append(new List<object> { room.Link, room.TableID });
        }

        public void RemoveMemberFromTeam(Room room, Team team, IMember member)
        {
            Table t = Tables.First(t => t.Name == room.Link);

            SendNativeQuery($"DELETE FROM {room.Link} WHERE ");
        }
    }
}
