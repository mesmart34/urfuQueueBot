using TableQueries;
using System;

namespace TableParser
{
    public class Member : IMember
    {
        public string Name { get; }

        public NotificationType Notification { get; }

        public event Action OnNotifyCalled;

        public Member(string name, NotificationType notificationType = NotificationType.TEN_MINUTES)
        {
            Name = name;
            Notification = notificationType;
        }

        public void Notify()
        {
            OnNotifyCalled?.Invoke();
        }
    }
}
