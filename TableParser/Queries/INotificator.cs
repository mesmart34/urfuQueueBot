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
