using System;
using System.IO;
using System.Text;

namespace Logger
{
    public class Logger : ILogger
    {
        private readonly string _path;
        public Logger(string path)
        {
            if (!path.EndsWith("/"))
            {
                throw new Exception("Path need ends with \"/\"");
            }

            _path = path;
        }

        public void Log(string text)
        {
            string today = DateTime.Today.ToString("dd-MM-yyyy");

            using var file = File.Open(_path + today + ".txt", FileMode.Append);
            file.Write(Encoding.UTF8.GetBytes(text + "\n"));
        }
    }
}
