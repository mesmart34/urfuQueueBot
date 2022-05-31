using System;
using System.Collections.Generic;

namespace SQLDB
{
    public class Team
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public List<Member> Members { get; private set; }
        public int Id { get; private set; }

        public Team(string name, int id, DateTime time)
        {
            Name = name;
            Time = time;
            Members = new List<Member>();
            Id = id;
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }

        public void AddStudent(Member student)
        {
            Members.Add(student);
        }

        public void RemoveStudent(Member student)
        {
            Members.Remove(student);
        }
    }
}
