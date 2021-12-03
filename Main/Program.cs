using System;
using System.IO;
using TableParser;
using TableQueries;
using TelegramBot;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            DefaultUpdateHandler updateHandler = new DefaultUpdateHandler();
            QueueBot.QueueBot bot;
            var dataBase = new DataBase();
            // TODO: Передать параметром реализацию IQuery
            var query = new Query(dataBase);
            using (StreamReader sr = new StreamReader("../../../../.private_cfg/token.txt"))
            {
                bot = new(sr.ReadLine(), updateHandler, query);
            }

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }
    }
}
