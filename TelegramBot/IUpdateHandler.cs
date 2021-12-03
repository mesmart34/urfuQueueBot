using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public interface IUpdateHandler : Telegram.Bot.Extensions.Polling.IUpdateHandler
    {
        public void AddResponse(
            Func<Message, bool> filter = null,
            List<string> commandsList = null,
            Func<Update, Task> response = null
            );

        public Func<Update, Task> GetQuery(
            IBot bot,
            string queryText,
            Func<Update, Task> response = null
            );
    }
}
