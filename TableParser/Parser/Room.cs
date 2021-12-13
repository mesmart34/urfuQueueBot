using System;
using System.Collections.Generic;

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

        public Room(string name, List<Team> teams, string link, DateTime start)
        {
            Name = name;
            Teams = teams;
            _link = link;
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
            //var key = Name + _tableID + StartTime.ToString("dd.MM HH:mm");

            var seed = Name.GetHashCode() + _tableID.GetHashCode() + StartTime.ToString("dd.MM HH:mm").GetHashCode();

            Random r = new Random(seed);
            
            string res = "";
            for (int i = 0; i < 50; ++i)
            {
                char c = (char)r.Next('a', 'z');
                res += (r.Next(0, 2) == 0 ? char.ToUpper(c) : c);
            }

            return Name + ":" + res;
        }
    }
}
