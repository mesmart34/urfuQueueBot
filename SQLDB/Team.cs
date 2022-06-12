using System;
using System.Collections.Generic;

namespace SQLDB
{
    public class Team
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }

        // TODO : Why Id
        public int Id { get; private set; }

        public HashSet<Student> Students { get; private set; }

        public Team(string name, DateTime time, int id)
        {
            Name = name;
            Time = time;
            Id = id;
            Students = new HashSet<Student>();
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }

        public void AddStudent(Student student) => Students.Add(student);

        public void RemoveStudent(Student student) => Students.Remove(student);
    }
}
