using System;
using TableParser;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace TableQueries
{
    public class Notification : INotificator
    {
        private bool _started;
        private readonly TableIO _table;
        private readonly Room _room;

        private readonly Color Yellow;

        public Notification(TableIO table, Room room)
        {
            _table = table;
            _room = room;
            Yellow = new Color
            {
                Red = 0.9843137f,
                Green = 0.7372549f,
                Blue = 0.01568628f
            };
        }

        private DateTime GetNistTime()
        {
            var client = new TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                var utcDateTimeString = response.Substring(7, 17);
                return DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        public void StartPolling()
        {
            _started = true;
            var th = new Thread(() =>
            {
                while (_started)
                {
                    var currentTime = GetNistTime();
                    //var colors = _table.LoadColors(_room.GetLink());
                    var activeTeamIndex = 0;
                    //foreach (var color in colors)
                    //{
                    //    if (color.Item2 == Yellow)
                    //    {
                    //        activeTeamIndex = color.Item1;
                    //    }
                    //}

                    foreach (var team in _room.Teams)
                    {
                        // get team index
                        int currentTeamIndex = Query.GetTeamIndex(_table, _room, team.Name);
                        foreach (Member member in team.Members)
                        {
                            var teamTime = team.Time;
                            switch (member.Notification)
                            {
                                case NotificationType.TEN_MINUTES:
                                    {
                                        var deltaTime = (currentTime - teamTime).TotalSeconds;
                                        if (deltaTime <= 600)
                                        {
                                            member.Notify();
                                        }
                                    }
                                    break;
                                case NotificationType.TWO_TEAMS:
                                    {
                                        if (currentTeamIndex - activeTeamIndex == 2)
                                        {
                                            member.Notify();
                                        }
                                    }
                                    break;
                                case NotificationType.AUTO:
                                    {
                                        // smth
                                    }
                                    break;
                            };
                        }
                    }
                    Thread.Sleep(90000); // 1.5 min
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        public void StopPolling()
        {
            _started = false;
        }
    }
}
