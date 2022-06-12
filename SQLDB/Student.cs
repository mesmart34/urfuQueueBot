using System;
using TableParser;

namespace SQLDB
{
    public class Student
    {
        public string Name { get; }
        public bool IsNotified { get; private set; }

        public NotificationType Notification { get; private set; }

        public event Action OnNotifyCalled;

        public Student(string name, NotificationType notificationType = NotificationType.TEN_MINUTES)
        {
            Name = name;
            Notification = notificationType;
        }

        public void Notify()
        {
            OnNotifyCalled?.Invoke();
            OnNotifyCalled = null;
            IsNotified = true;
        }

        public void SetNotificationType(NotificationType type)
        {
            Notification = type;
        }
    }
}
