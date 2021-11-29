using System;
using System.IO;
using TelegramBot;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            DefaultUpdateHandler updateHandler = new DefaultUpdateHandler();
            QueueBot.QueueBot bot;

            // TODO: Передать параметром реализацию IQuery

            using (StreamReader sr = new StreamReader("../../../../.private_cfg/token.txt"))
            {
                bot = new(sr.ReadLine(), updateHandler);
            }

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }
    }
}
