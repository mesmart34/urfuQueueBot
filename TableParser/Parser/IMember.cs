using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    // TODO: Реализовать IMember

    public interface IMember
    {
        string Name { get; }
        NotificationType Notification { get; }

        void SetNotificationType(NotificationType type);

        void Notify();
    }
}
