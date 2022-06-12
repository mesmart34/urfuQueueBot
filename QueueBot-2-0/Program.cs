using System;
using BotBuilder;
using Bots;
using Bots.TG;
using Bots.VK;
using TableParser;
//using SQLDB;

namespace QueueBot_2_0
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data(new DataBase("1GISEntayuaYagp7K9Zb13-YLGiDO6AaQIVfP39REkI0"));

            TGBot tgbot = new(
                    new Token("../../../../.private_cfg/tgtoken.txt"),
                    new Bots.UpdateHandlers.TG.TGUpdateHandler(),
                    data
                //new IO("SQLEXPRESS", "Rooms")
                );

            tgbot.LoadStickerSet("expert", "KattBot");
            tgbot.LoadStickerSet("student", "Urfu_Robot");

            VKBot vkbot = new(
                    new Token("../../../../.private_cfg/vktoken.txt"),
                    new Bots.UpdateHandlers.VK.VKUpdateHandler(),
                    data
                );

            // TODO : Start in different threads

            Builder tgBuilder = new Builder();
            Builder vkBuilder = new Builder();
            tgBuilder.Build(tgbot);
            vkBuilder.Build(vkbot);

            tgbot.StartReceiving();
            vkbot.StartReceiving();

            Console.ReadLine();

            tgbot.StopReceiving();
            vkbot.StopReceiving();
        }
    }
}
