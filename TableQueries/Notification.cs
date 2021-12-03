using System;
using TableParser;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Globalization;
using Google.Apis.Sheets.v4.Data;

namespace TableQueries
{
    public class Notification : INotificator
    {
        private bool _started;
        ///private DataBase _dataBase;
        private TableIO _table;
        private List<IMember> _members;
        private Room _room;

        readonly Color Yellow;

        public Notification(TableIO table, Room room)
        {
            _table = table;
            _members = new List<IMember>();
            _room = room;
            Yellow = new Color();
            Yellow.Red = 0.9843137f;
            Yellow.Green = 0.7372549f;
            Yellow.Blue = 0.01568628f;
        }

        public void Subscribe(IMember member)
        {
            _members.Add(member);
        }

        public void Unsubscribe(IMember member)
        {
            _members.Remove(member);
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
                    var colors = _table.LoadColors(_room.GetLink());
                    var currentTeamIndex = 0;
                    foreach (var color in colors)
                    {
                        if (color.Item2 == Yellow)
                        {
                            currentTeamIndex = color.Item1;
                        }
                    }
                    foreach (Member member in _members)
                    {
                        var teamTime = member.Team.Time;
                        switch (member.Notification)
                        {
                            case NotificationType.TEN_MINUTES:
                                {
                                    var deltaTime = (currentTime - teamTime).TotalSeconds / 60;
                                    if (deltaTime <= 10)
                                    {
                                        //Вызвать уведомление
                                    }
                                }
                                break;
                            case NotificationType.TWO_TEAMS:
                                {
                                    if (member.Team.Id - currentTeamIndex == 2)
                                    {
                                        //Вызвать уведомление
                                    }
                                }
                                break;
                            case NotificationType.AUTO:
                                {

                                }
                                break;
                        };
                    }
                    Thread.Sleep(90000);
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
