using System;
using System.Collections.Generic;

namespace TableParser
{
    public class Room
    {
        //public string Name { get; private set; }
        public int Number { get; }
        public DateTime StartTime { get; }
        public List<Team> Teams { get; private set; }

        public string TableID { get; }

        public Room(int number, string tableID, List<Team> teams, DateTime start)
        {
            Number = number;
            Teams = teams;
            TableID = tableID;
            StartTime = start;
        }

        public Room(int number, List<Team> teams, string link, DateTime start)
        {
            Number = number;
            Teams = teams;
            _link = link;
            StartTime = start;
        }

        public string Name => "Комната " + Number.ToString();

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
            var seed = Number.GetHashCode() + TableID.GetHashCode() + StartTime.ToString("dd.MM HH:mm").GetHashCode();

            Random r = new Random(seed);
            
            string res = "";
            for (int i = 0; i < 20; ++i)
            {
                char c = (char)r.Next('a', 'z');
                res += (r.Next(0, 2) == 0 ? char.ToUpper(c) : c);
            }

            // [Number][Hash]
            return Number.ToString() + res;
        }
    }
}
