using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class Program
    {
        static Dictionary<string, Room> Links = new Dictionary<string, Room>();
        static TableParser UserDataBase = new TableParser("1bUR8BaBnYjc5rkaC9DHW6vm79qu7UVGgqBOVuZgeMAc");

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var parser = new TableParser("1mM1JgYBx188-fujNFJIfgWQPDm5QyvcSwFjsMCEDJzY");
            var rooms = parser.Parse();
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
            UpdateDataBase(rooms);
            Console.ReadLine();

        }

        static void UpdateDataBase(List<Room> rooms)
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
/*                var toAdd = new List<IList<object>>();
                toAdd.Add(new List<object>() { studentName });
                UserDataBase.Write(link, "A1", toAdd);
*/
                Console.WriteLine(studentName + " is connected to " + room.Name);
                return room;
            }
            Console.WriteLine("Room doesnt exist: " + link);
            return null;
        }


    }
}
