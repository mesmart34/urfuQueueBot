using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace urfuQueueBot
{
    class Team
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public List<string> Students { get; private set; }

        public Team(string name, DateTime time)
        {
            Name = name;
            Time = time;
            Students = new List<string>();
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }
        
        public void AddStudent(string student)
        {
            Students.Add(student);
        }
    }
}
