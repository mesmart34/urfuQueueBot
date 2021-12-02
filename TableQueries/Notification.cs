using System;
using TableParser;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Globalization;

namespace TableQueries
{
    public class Notification : INotificator
    {
        private event Action _onUpdate;
        private bool _started;
        private DataBase _dataBase;
        private List<IMember> _members;

        public Notification(DataBase dataBase)
        {
            _dataBase = dataBase;
            _members = new List<IMember>();
        }

        public void Subscribe(IMember member)
        {
            _members.Add(member);
        }

        public void Unsubscribe(Action action)
        {
            _onUpdate -= action;
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
                    var req = WebRequest.CreateHttp("https://worldtimeapi.org/api/timezone/Europe");
                    req.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => certificate.GetCertHashString() == "<real_Hash_here>";
                    var response = req.GetResponse();
                    foreach (var member in _members)
                    {
                        switch (member.Notification)
                        {
                            case NotificationType.TEN_MINUTES:
                                {
                                    
                                }
                                break;
                            case NotificationType.TWO_TEAMS:
                                {

                                }
                                break;
                            case NotificationType.AUTO:
                                {

                                }
                                break;
                        };
                    }
                    _onUpdate?.Invoke();
                    Thread.Sleep(1500);
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
