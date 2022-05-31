using System.Collections.Generic;
using System.Linq;
using TableParser;
//using SQLDB;

namespace Bots
{
    public struct Session
    {
        public Member Member;
        public Room Room;
        public Team Team;
    }

    public class Data
    {
        public List<Session> Sessions { get; }

        public List<INotificator> Notificators { get; }

        public DataBase DataBase { get; }

        public Data(DataBase db)
        {
            Sessions = new List<Session>();
            DataBase = db;
            Notificators = new List<INotificator>();

            // TODO : delete
            foreach (var room in db.GetRooms())
            {
                foreach (var team in room.Teams)
                {
                    foreach (var member in team.Members)
                    {
                        //member.OnNotifyCalled += () => SendMessage(long.Parse(member.Name), "Подключайтесь!");
                    }
                }

                var not = new Notificator(room);
                not.StartPolling();
                Notificators.Add(not);
            }
        }
    }
}

//using System.Collections.Generic;
//using TableParser;
//using System.Linq;
////using SQLDB;

//namespace Bots
//{
//    public struct Session
//    {
//        public IMember Member;
//        public Room Room;
//        public Team Team;
//    }

//    public class BotData
//    {
//        public List<Session> Sessions { get; }

//        public List<INotificator> Notificators { get; }
//        //public IO DataBase { get; }
//        public DataBase DataBase { get; }

//        // TODO : delete
//        private readonly List<Room> _rooms;

//        public BotData(DataBase db)
//        {
//            Sessions = new List<Session>();
//            DataBase = db;
//            Notificators = new List<INotificator>();
//            _rooms = db.GetRooms().ToList();

//            // TODO : delete
//            foreach (var room in _rooms)
//            {
//                foreach (var team in room.Teams)
//                {
//                    foreach (var member in team.Members)
//                    {
//                        //member.OnNotifyCalled += () => SendMessage(long.Parse(member.Name), "Подключайтесь!");
//                    }
//                }

//                var not = new Notificator(room);
//                not.StartPolling();
//                Notificators.Add(not);
//            }
//        }
//    }
//}
