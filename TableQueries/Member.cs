namespace TableQueries
{
    public class Member : IMember
    {
        public string Name { get; }

        public NotificationType Notification { get; }

        public Member(string name, NotificationType notificationType)
        {
            Name = name;
            Notification = notificationType;
        }
    }
}
