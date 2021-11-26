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
                var room = RoomParser.CreateRoom(roomsTable, sheet);
                var link = room.GetLink();
                Rooms.Add(link, room);
            }
            var dataBase = new DataBase("1bUR8BaBnYjc5rkaC9DHW6vm79qu7UVGgqBOVuZgeMAc");
            var time = DateTime.Parse("1/15/2022 17:40:00");
            var team = new Team("Trex", time);
            team.AddStudent("Биборан");
            team.AddStudent("Масленок");
            team.AddStudent("Питух");
            Rooms["0JrQvtC80L3QsNGC0LAgMzFtTTFKZ1lCeDE4OC1mdWpORkpJZmdXUVBEbTVReXZjU3dGanNNQ0VESnpZ"].AddTeam(team, time);
            dataBase.UpdateWhole(Rooms);
            dataBase.Read(Rooms);
            Console.ReadLine();
        }
    }
}
