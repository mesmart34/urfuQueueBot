using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Globalization;
using Google.Apis.Sheets.v4.Data;
using TableParser.Parser;

namespace TableParser
{
    public class Notificator : INotificator
    {
        private bool _started;
        private readonly TableIO _table;
        private readonly Room _room;

        private float totalTimeMinutes = 0f;
        private int totalPassedTeams = 0;

        private readonly Color Green = new Color
        {
            Red = 0.203921571f,
            Green = 0.65882355f,
            Blue = 0.3254902f
        };

        private readonly Color Yellow = new Color
        {
            Red = 0.9843137f,
            Green = 0.7372549f,
            Blue = 0.01568628f
        };

        private readonly Color Red = new Color
        {
            Red = 0.917647064f,
            Green = 0.2627451f,
            Blue = 0.20784314f
        };

        public Notificator(TableIO table, Room room)
        {
            _table = table;
            _room = room;
        }

        public Notificator(Room room)
        {
            _table = new TableIO(room.TableID);
            _room = room;
        }

        private DateTime GetNistTime()
        {
            var client = new TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                try
                {
                    var response = streamReader.ReadToEnd();
                    var utcDateTimeString = response.Substring(7, 17);
                    return DateTime.Parse(utcDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }
                catch
                {
                    return DateTime.Now;
                }
            }
        }

        public void StartPolling()
        {
            // TODO: refac
            _started = true;
            var th = new Thread(() =>
            {
                int prevActiveTeamIndex = -1;
                DateTime prevTeamStartTime = _room.StartTime;
                while (_started)
                {
                    var currentTime = GetNistTime();

                    var colors = _table.LoadColors(_room.Name);

                    var activeTeamIndex = 0;

                    var data = _table.Read(_room.Name);
                    var firstRoomDef = TableIO.GetRoomCellIdByContent(data, "Защита", 0);
                    var secondRoomDef = TableIO.GetRoomCellIdByContent(data, "Защита", 1);

                    int skipRows = 0;
                    int offset = 0;
                    for (; offset < data[0].Count; ++offset)
                    {
                        var roomInfoRow = TableIO.GetRoomCellIdByContent(data, "Защита", offset);
                        var roomInfo = ((string)data[roomInfoRow][offset]).Split(' ');
                        var timeStr = roomInfo[1] + " " + roomInfo[3];
                        var time = DateTime.ParseExact(timeStr, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        if (time == _room.StartTime)
                        {
                            skipRows = roomInfoRow * data[0].Count + 2;
                            break;
                        }
                    }

                    foreach (var color in colors.Skip(skipRows))
                    {
                        if (color.Item1 == offset && color.Item3.EqualTo(Yellow))
                        {
                            if (prevActiveTeamIndex != -1)
                            {
                                totalPassedTeams += 1;
                                totalTimeMinutes += (float)(GetNistTime() - prevTeamStartTime).TotalMinutes;
                            }
                            prevTeamStartTime = GetNistTime();
                            // color.Item2 - roomInfoRow
                            activeTeamIndex = color.Item2 - (skipRows - 2) / data[0].Count - 2;
                            prevActiveTeamIndex = activeTeamIndex;
                            break;
                        }
                    }

                    foreach (var team in _room.Teams)
                    {
                        int currentTeamIndex = team.Id;

                        foreach (Member member in team.Members)
                        {
                            if (member.IsNotified)
                                continue;

                            var teamTime = team.Time;
                            switch (member.Notification)
                            {
                                // TODO: fill rules
                                case NotificationType.TEN_MINUTES:
                                    {
                                        var deltaTime = (teamTime - currentTime).TotalSeconds;
                                        int notificationOffset = 20; //s
                                        // int notificationOffset = 10 * 60 //s == 10 min
                                        // TODO: Add timer-task
                                        if (deltaTime >= -(60 * 1.5 + 1) && deltaTime <= notificationOffset)
                                        {
                                            member.Notify();
                                        }
                                    }
                                    break;
                                case NotificationType.TWO_TEAMS:
                                    {
                                        if (currentTeamIndex - activeTeamIndex <= 2)
                                        {
                                            member.Notify();
                                        }
                                    }
                                    break;
                                case NotificationType.AUTO:
                                    {
                                        float avgTimeSeconds = 60 * totalTimeMinutes / totalPassedTeams;
                                        DateTime currentTeamStartTime = _room.StartTime.AddSeconds(team.Id * avgTimeSeconds);
                                        var deltaTime = (currentTeamStartTime - prevTeamStartTime).TotalSeconds;
                                        // prevTeamStartTime + avgTime * team.Id 
                                        // TODO: Add timer-task
                                        if (deltaTime <= avgTimeSeconds && deltaTime >= -(60 * 1.5 + 1))
                                        {
                                            member.Notify();
                                        }
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
