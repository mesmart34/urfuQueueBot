using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using SendableFiles;
using TelegramBot.UpdateHandler;

namespace TelegramBot
{
    public interface IBot
    {
        BotData Data { get; }

        IUpdateHandler UpdateHandler { get; }

        void StartReceiving();
        void StopReceiving();

        void SendMessageAsync(ChatId chatId, string text = null, IEnumerable<ISendable> content = null, Keyboard keyboard = null);
        void InvokeMessage(Message mes);
    }
}
