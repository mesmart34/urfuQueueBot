using System;
using System.IO;
using TableParser;
using TelegramBot;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            DefaultUpdateHandler updateHandler = new DefaultUpdateHandler();
            QueueBot.QueueBot bot;
            using (StreamReader sr = new StreamReader("../../../../.private_cfg/token.txt"))
            {
                bot = new(sr.ReadLine(), updateHandler, new DataBase("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0"));
            }

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }
    }
}
