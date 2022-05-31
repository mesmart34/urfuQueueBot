using System.IO;

namespace Bots
{
    public class Token
    {
        private readonly string _path;

        public Token(string path)
        {
            _path = path;
        }

        public string GetToken()
        {
            using StreamReader sr = new StreamReader(_path);
                return sr.ReadLine();
        }
    }
}
