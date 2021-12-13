namespace TableParser
{
    public enum NotificationType
    {
        TEN_MINUTES,
        TWO_TEAMS,
        AUTO
    }

    public interface INotificator
    {
        void StartPolling();
        void StopPolling();
    }
}
