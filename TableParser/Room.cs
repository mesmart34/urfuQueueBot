using System;
using System.Collections.Generic;
using System.Text;

namespace TableParser
{
    public class Room
    {
        public string Name { get; private set; }
        public Dictionary<DateTime, List<Team>> Teams { get; private set; }
        private string _tableID;

        public Room(string name, string tableID, Dictionary<DateTime, List<Team>> team)
        {
            Name = name;
            Teams = team;
            _tableID = tableID;
        }

        public void AddTeam(Team team, DateTime time)
        {
            if (!Teams.ContainsKey(time))
                Teams.Add(time, new List<Team>());
            Teams[time].Add(team);
        }

        public string GetLink()
        {
            var key = Name + _tableID;
            var bytes = Encoding.UTF8.GetBytes(key);
            var encoded = Convert.ToBase64String(bytes);
            return encoded;
        }
    }
}
