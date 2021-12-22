using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using SendableFiles;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBot
{
    public class Bot : IBot
    {
        protected readonly TelegramBotClient _botClient;
        private CancellationTokenSource _cancellationToken;

        protected IUpdateHandler _updateHandler;

        public Bot(string token, IUpdateHandler updateHandler)
        {
            _botClient = new TelegramBotClient(token);
            _cancellationToken = new CancellationTokenSource();
            _updateHandler = updateHandler;
        }

        public Func<Update, Task> SendMessageResponse(
            string text = null, 
            List<ISendable> files = null, 
            IReplyMarkup keyboard = null
            )
        {
            Task ResponseTask(Update update)
            {
                List<Task> tasks = new List<Task>();

                ChatId chatId = update.Message.Chat.Id;

                if (text != null)
                {
                    tasks.Add(_botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: text,
                        replyMarkup: keyboard ?? Keyboard.RemoveMarkup
                        )
                    );
                }

                if (files == null) return tasks.First();

                foreach (var file in files)
                {
                    tasks.Add(file.Send(_botClient, chatId));
                }

                return Task.WhenAll(tasks);
            }

            return ResponseTask;
        }

        public Task SendMessage(ChatId chatId, string text, IReplyMarkup replyMarkup = null)
        {
            return _botClient.SendTextMessageAsync(chatId, text, replyMarkup: replyMarkup ?? Keyboard.RemoveMarkup);
        }

        public Task SendMessageTest(ChatId chatId, string text = null, IEnumerable<ISendable> content = null, Keyboard keyboard = null)
        {
            List<Task> tasks = new List<Task>();

            if (text != null)
            {
                tasks.Add(_botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: keyboard?.GetReplyMarkup() ?? Keyboard.RemoveMarkup
                    )
                );
            }

            if (content == null) return tasks.First();

            foreach (var file in content)
            {
                tasks.Add(file.Send(_botClient, chatId));
            }

            return Task.WhenAll(tasks);
        }

        public Task SendSticker(ChatId chatId, string stId)
        {
            var st = new InputTelegramFile(stId);
            return _botClient.SendStickerAsync(chatId, st.FileId);
        }

        public StickerSet GetStickerSet(string shortName)
        {
            return _botClient.GetStickerSetAsync(shortName).Result;
        }

        public void StartReceiving()
        {
            _botClient.StartReceiving(
                _updateHandler,
                new ReceiverOptions { AllowedUpdates = { } },
                _cancellationToken.Token
                );
        }

        public void StopReceiving()
        {
            _cancellationToken.Cancel();
        }
    }
}
