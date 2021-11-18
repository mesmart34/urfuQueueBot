using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class Room
    {
        public string Name { get; private set; }
        public Dictionary<DateTime, List<Team>> Teams { get; private set; }

        public Room(string name, Dictionary<DateTime, List<Team>> team)
        {
            Name = name;
            Teams = team;
        }

        public void AddTeam(Team team, DateTime time)
        {
            if (!Teams.ContainsKey(time))
                Teams.Add(time, new List<Team>());
            Teams[time].Add(team);
        }

        public string GetLink()
        {
            var hash = Name.GetHashCode();
            var random = new Random(hash);
            var builder = new StringBuilder();
            for(var i = 0; i < 10; i++)
            {
                var code = (char)(random.Next('A', 'Z'));
                if (random.Next(0, 2) == 0)
                    code = char.ToLower(code);
                builder.Append(code);
            }
            return builder.ToString();
        }


    }
}
