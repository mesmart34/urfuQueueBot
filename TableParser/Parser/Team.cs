using System;
using System.Collections.Generic;

namespace TableParser
{
    public class Team
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public List<IMember> Members { get; private set; }
        public int Id { get; private set; }

        public Team(string name, int id, DateTime time)
        {
            Name = name;
            Time = time;
            Members = new List<IMember>();
            Id = id;
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }

        public void AddStudent(IMember student)
        {
            Members.Add(student);
        }

        public void RemoveStudent(IMember student)
        {
            Members.Remove(student);
        }
    }
}
