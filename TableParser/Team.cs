﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public class Team
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }
        public List<string> Students { get; private set; }
        public int Id { get; private set; }

        public Team(string name, int id, DateTime time)
        {
            Name = name;
            Time = time;
            Students = new List<string>();
            Id = id;
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }

        public void AddStudent(string student)
        {
            Students.Add(student);
        }

        public void RemoveStudent(string student)
        {
            foreach(var stu in Students)
            {
                if(stu == student)
                    Students.Remove(stu);
            }
        }
    }
}
