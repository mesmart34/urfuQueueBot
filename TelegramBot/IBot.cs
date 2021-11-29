using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using FileManager;

namespace TelegramBot
{
    public interface IBot
    {
        public void StartReceiving();
        public void StopReceiving();

        public Func<Update, Task> SendMessageResponse(string text = null, List<IFile> files = null, IReplyMarkup keyboard = null);

        public Task SendMessage(ChatId chatId, string text, IReplyMarkup replyMarkup = null);
    }
}
