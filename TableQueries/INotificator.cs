using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableQueries
{
    // TODO: Заполнить типы уведомлений
    public enum NotificationType
    {

    }

    // TODO: Реализовать INotificator
    public interface INotificator
    {
        void SetNotificationType(NotificationType type);

        void StartPolling();
        void StopPolling();
    }
}
