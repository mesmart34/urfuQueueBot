using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Collections.Generic;

namespace TelegramBot.UpdateHandler
{
    public interface IUpdateHandler : Telegram.Bot.Extensions.Polling.IUpdateHandler
    {
        Task InvokeMessage(ITelegramBotClient botClient, Message message);

        public void AddResponse(Response response);

        void RemoveResponse(Response response);

        void AddQuery(Update upd, Response.ResponseFunc response);
    }
}
