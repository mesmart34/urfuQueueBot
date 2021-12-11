using System;
using System.Collections.Generic;
using System.Text;

namespace TableParser
{
    public class Room
    {
        public string Name { get; private set; }
        public DateTime StartTime { get; }
        public List<Team> Teams { get; private set; }
        private readonly string _tableID;

        public Room(string name, string tableID, List<Team> teams, DateTime start)
        {
            Name = name;
            Teams = teams;
            _tableID = tableID;
            StartTime = start;
        }

        public void AddTeam(Team team)
        {
            if (!Teams.Contains(team))
                Teams.Add(team);
        }

        private string _link;
        public string Link
        {
            get
            {
                if (_link == null)
                    _link = GetLink();
                return _link;
            }
        }

        private string GetLink()
        {
            var key = Name + _tableID + StartTime.ToString("dd.MM HH:mm");
            var bytes = Encoding.UTF8.GetBytes(key);
            var encoded = Convert.ToBase64String(bytes);
            return encoded;
        }
    }
}
