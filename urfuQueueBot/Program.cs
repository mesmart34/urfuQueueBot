using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{ 
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new TableParser("1mM1JgYBx188-fujNFJIfgWQPDm5QyvcSwFjsMCEDJzY");
            var rooms = parser.Parse();
            /*var sheets = parser.GetAllSheets();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var rooms = new List<Room>();
            foreach (var sheet in sheets)
            {
                var mode = ReadMode.None;
                var currentRoom = new Room();
                var result = parser.Read(sheet);
                foreach (var data in result)
                {
                    var value = data[0].ToString();
                    if (value == string.Empty || value == "*")
                        continue;
                    if (value == "Эксперты")
                    {
                        mode = ReadMode.ExpertMode;
                    }
                    else if (value == "Модераторы")
                    {
                        mode = ReadMode.ModeratorMode;

                    }
                    else if (value.StartsWith("Комната"))
                    {
                        mode = ReadMode.TeamMode;
                        currentRoom.name = value;
                    }
                    else
                    {
                        switch (mode)
                        {
                            case ReadMode.ExpertMode:
                                {
                                    var expert = new Expert();
                                    expert.Name = value;
                                    currentRoom.experts.Add(expert);
                                }
                                break;
                            case ReadMode.ModeratorMode:
                                {
                                    var moderator = new Moderator();
                                    moderator.Name = value;
                                    currentRoom.moderators.Add(moderator);
                                }
                                break;
                            case ReadMode.TeamMode:
                                {
                                    var team = new Team();
                                    team.Name = value;
                                    currentRoom.teams.Add(team);
                                }
                                break;
                        }
                    }
                }
                rooms.Add(currentRoom);
            }*/
            Console.ReadLine();

        }
    }
}
