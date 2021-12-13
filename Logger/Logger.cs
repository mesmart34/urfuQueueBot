using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Logger
{
    public class Logger : ILogger
    {
        private string _path;
        public Logger(string path)
        {
            if (!path.EndsWith("/"))
            {
                throw new Exception("Path need ends with \"\\\"");
            }

            _path = path;
        }

        public void Log(string text)
        {
            DirectoryInfo dir = new DirectoryInfo(_path);
            var filesNames = dir.GetFiles().Select(f => f.Name);
            string today = DateTime.Today.ToString("dd-MM-yyyy");

            using (var file = File.Open(_path + today + ".txt", FileMode.Append))
            {
                file.Write(Encoding.UTF8.GetBytes(text + "\n"));
            }
        }
    }
}
