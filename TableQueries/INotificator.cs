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
        TEN_MINUTES,
        TWO_TEAMS,
        AUTO
    }

    // TODO: Реализовать INotificator
    public interface INotificator
    {
        void StartPolling();
        void StopPolling();
    }
}
