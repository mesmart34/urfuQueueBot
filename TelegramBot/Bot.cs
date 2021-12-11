using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using FileManager;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;
using FileType = FileManager.FileType;

namespace TelegramBot
{
    public class Bot : IBot
    {
        private readonly TelegramBotClient _botClient;
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
            List<IFile> files = null, 
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

                // TODO: Добавить обработку стикеров
                foreach (var file in files)
                {
                    Stream sr = File.OpenRead(file.Path);
                    tasks.Add(
                        file.FileType switch
                        {
                            FileType.Image => _botClient.SendPhotoAsync(chatId, sr, file.Caption),
                            FileType.Document => _botClient.SendDocumentAsync(chatId, new InputOnlineFile(sr), caption: file.Caption),
                            FileType.Video => _botClient.SendVideoAsync(chatId, new InputOnlineFile(sr), caption: file.Caption),
                            FileType.Audio => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                    );
                }

                return Task.WhenAll(tasks);
            }

            return ResponseTask;
        }

        public Task SendMessage(ChatId chatId, string text, IReplyMarkup replyMarkup = null)
        {
            return _botClient.SendTextMessageAsync(chatId, text, replyMarkup: replyMarkup ?? Keyboard.RemoveMarkup);
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
