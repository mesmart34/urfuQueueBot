using TableParser;

namespace TableQueries
{
    public class Member : IMember
    {
        public string Name { get; }
        public Team Team { get; private set; }
         
        public NotificationType Notification { get; }

        public Member(string name, Team team, NotificationType notificationType)
        {
            Name = name;
            Notification = notificationType;
            Team = team;
        }
    }
}
