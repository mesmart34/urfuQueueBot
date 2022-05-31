using System;
using System.IO;
using System.Text;

namespace Loggers
{
    public class Logger
    {
        private readonly string _logsPath;
        public Logger(string path)
        {
            if (!path.EndsWith("/"))
            {
                throw new Exception("Path must ends with \"/\"");
            }

            _logsPath = path;
        }

        public void Log(string text)
        {
            string today = DateTime.Today.ToString("dd-MM-yyyy");

            using var file = File.OpenWrite(_logsPath + today + ".txt");
            file.Write(Encoding.UTF8.GetBytes(text + "\n"));
        }
    }
}
