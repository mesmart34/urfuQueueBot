using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class Program
    {
        static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var roomsTable = new TableIO("1mM1JgYBx188-fujNFJIfgWQPDm5QyvcSwFjsMCEDJzY");
            foreach(var sheet in roomsTable.GetAllSheets())
            {
                var roomData = roomsTable.Read(sheet.Name);
                var room = RoomParser.GetRoom(roomData);
                var link = room.GetLink();
                Rooms.Add(link, room);
            }
            var dataBase = new DataBase("1bUR8BaBnYjc5rkaC9DHW6vm79qu7UVGgqBOVuZgeMAc");


            var time = DateTime.Now;
            var team = new Team("Trex", time);
            team.AddStudent("Биборан");
            team.AddStudent("Масленок");
            team.AddStudent("Питух");
            Rooms["siubiAxfKP"].AddTeam(team, time);

            dataBase.UpdateWhole(Rooms);
            //dataBase.Read(Rooms);
            /*var rooms = parser.Read
            foreach (var r in rooms)
            {
                var link = r.GetLink();
                Links.Add(link, r);
                UserDataBase.CreateSheet(link);
                Console.WriteLine(link);
            }
            var room1 = Connect("trxqqxxr", "Артём Екимов");
            var room2 = Connect("uzzzyyrs", "Артём Екимов");
            CreateTeam(room1, "Тиранозавры", DateTime.Now);
            CreateTeam(room2, "Тиранозавры", DateTime.Now);
            UpdateDataBase(rooms);*/
            Console.ReadLine();

        }

        /*static void UpdateDataBase(List<Room> rooms)
        {
            foreach (var room in rooms)
            {
                var roomData = new List<IList<object>>();
                foreach(var timeKey in room.Teams.Keys)
                {
                    var row = new List<object>();
                    row.Add(timeKey.ToString());
                    foreach (var team in room.Teams[timeKey])
                    {
                        row.Add(team.Name);
                    }
                    roomData.Add(row);
                } 
                UserDataBase.Write(room.GetLink(), roomData);
            }
        }

        static void CreateTeam(Room room, string name, DateTime time)
        {
            var team = new Team();
            team.Name = name;
            team.Time = time;
            room.AddTeam(team, time);
            Console.WriteLine("Team " + name + " is added to " + time.ToString());
        }

        static Room Connect(string link, string studentName)
        {
            if (Links.ContainsKey(link))
            {
                var room = Links[link];
*//*                var toAdd = new List<IList<object>>();
                toAdd.Add(new List<object>() { studentName });
                UserDataBase.Write(link, "A1", toAdd);
*//*
                Console.WriteLine(studentName + " is connected to " + room.Name);
                return room;
            }
            Console.WriteLine("Room doesnt exist: " + link);
            return null;
        }*/


    }
}
