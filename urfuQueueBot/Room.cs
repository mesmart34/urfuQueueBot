using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    struct Team
    {
        public string Name;
        public DateTime Time;
    }

    class Room
    {
        public string Name { get; private set; }
        public Dictionary<DateTime, List<Team>> Teams { get; private set; }

        public Room(string name, Dictionary<DateTime, List<Team>> team)
        {
            Name = name;
            Teams = team;
        }

        public string GetLink()
        {
            var hash = GetHashCode().ToString();
            var builder = new StringBuilder();
            foreach(var c in hash)
            {
                var code = (char)(c + 'A');
                builder.Append(code);
            }
            return builder.ToString();
        }


    }
}
