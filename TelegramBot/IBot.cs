using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using SendableFiles;

namespace TelegramBot
{
    public interface IBot
    {
        public void StartReceiving();
        public void StopReceiving();

        public Func<Update, Task> SendMessageResponse(string text = null, List<ISendable> content = null, IReplyMarkup keyboard = null);

        public Task SendMessage(ChatId chatId, string text, IReplyMarkup replyMarkup = null);
        public Task SendMessageTest(ChatId chatId, string text = null, IEnumerable<ISendable> content = null, Keyboard keyboard = null);
    }
}
