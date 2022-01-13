using System.IO;

namespace TelegramBot
{
    public class BotToken
    {
        private string _path;

        public BotToken(string path)
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
