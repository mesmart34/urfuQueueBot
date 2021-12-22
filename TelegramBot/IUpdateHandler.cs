using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public interface IUpdateHandler : Telegram.Bot.Extensions.Polling.IUpdateHandler
    {
        public Task InvokeMessage(ITelegramBotClient botClient, Message message);

        public void AddResponse(
            Func<Message, bool> filter = null,
            Func<Update, Task> response = null
            );

        public Func<Update, Task> GetQuery(
            Func<Update, Task> response = null
            );
    }
}
