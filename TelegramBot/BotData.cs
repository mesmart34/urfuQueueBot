using System.Collections.Generic;
using TableParser;
using System.Linq;

namespace TelegramBot
{
    public struct Session
    {
        public IMember Member;
        public Room Room;
        public Team Team;
    }

    public class BotData
    {
        public List<Session> Sessions { get; }

        public List<INotificator> Notificators { get; }
        public DataBase DataBase { get; }

        private readonly List<Room> _rooms;

        public BotData(DataBase db)
        {
            Sessions = new List<Session>();
            DataBase = db;
            Notificators = new List<INotificator>();
            _rooms = db.GetRooms().ToList();

            // delete
            foreach (var room in _rooms)
            {
                if (room.Name != "Комната 1")
                    continue;

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
