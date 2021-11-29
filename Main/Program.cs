using System;
using System.Collections.Generic;
using System.IO;
using TelegramBot;
using FileManager;
using Telegram.Bot.Types;
using QueueBot;
using System.Threading.Tasks;
using System.Linq;

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
                bot = new(sr.ReadLine(), updateHandler);
            }

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }
    }
}
