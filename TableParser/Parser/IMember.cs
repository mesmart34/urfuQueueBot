using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public interface IMember
    {
        string Name { get; }
        NotificationType Notification { get; }
        public event Action OnNotifyCalled;

        void SetNotificationType(NotificationType type);

        void Notify();
    }
}
