using System;
using TableParser;
using TelegramBot;
using TelegramBot.UpdateHandler;
using BotConstructor;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new(
                    new BotToken("../../../../.private_cfg/token.txt"),
                    new DefaultUpdateHandler(),
                    new DataBase("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0")
                );

            IBuilder builder = new Builder(bot);
            builder.Build();

            bot.StartReceiving();

            Console.ReadLine();

            bot.StopReceiving();
        }
    }
}
