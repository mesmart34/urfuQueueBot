using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{

    struct Expert
    {
        public Expert(string name)
        {
            Name = name;
        }

        public string Name;
    }

    struct Moderator
    {   
        public Moderator(string name)
        {
            Name = name;
        }

        public string Name;
    }

    struct Student
    {
        public string Name;
        public string Group;
    }

    struct Team
    {
        public string Name;
        public List<Student> Students;
    }

    class Room
    {
        public List<Expert> experts;
        public List<Moderator> moderators;
        public Dictionary<DateTime, List<Team>> teams;
        public string name;


        public Room()
        {
            experts = new List<Expert>();
            teams = new Dictionary<DateTime, List<Team>>();
            moderators = new List<Moderator>();
        }

    }
}
