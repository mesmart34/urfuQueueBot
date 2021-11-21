using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using FileManager;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;
using FileType = FileManager.FileType;
using urfuQueueBot;

namespace Bot
{
    public class BotClass
    {
        private readonly TelegramBotClient _botClient;
        private CancellationTokenSource _cancellationToken;

        private Func<Update, Task> _updateHandle;
        private Dictionary<ChatId, Func<Update, Task>> _queriedChatIds;

        public BotClass(string token)
        {
            _botClient = new TelegramBotClient(token);
            _cancellationToken = new CancellationTokenSource();
            _queriedChatIds = new Dictionary<ChatId, Func<Update, Task>>();
        }

        private async Task DefaultHandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            await Task.Run(() => Console.WriteLine(errorMessage));
            RestartReceiving();
        }

        private async Task DefaultHandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            if (update.Message.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var message = update.Message.Text;

            Console.WriteLine($"Received a message '{update.Message.Text}' in chat @{update.Message.From.Username} [{chatId}].");

            if (!_queriedChatIds.ContainsKey(chatId))
            {
                await _updateHandle(update);
            }
            else
            {
                await _queriedChatIds[chatId](update);
                _queriedChatIds.Remove(chatId);
            }
        }

        public Func<Update, Task> GetRoomsResponse()
        {
            Task RoomsResponse(Update update)
            {
                var roomsTable = new TableIO(update.Message.Text);
                var rooms = roomsTable.GetAllSheets();

                return _botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: String.Join("\n", (rooms.Select(room => $"{room.Name} - {new Room(room.Name, null).GetLink()}"))),
                        replyMarkup: new ReplyKeyboardRemove()
                        );
            }

            return RoomsResponse;
        }

        public Func<Update, Task> SendMessage(string text = null, List<IFile> files = null, IReplyMarkup keyboard = null)
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
                        replyMarkup: keyboard ?? new ReplyKeyboardRemove()
                        )
                    );
                }

                if (files == null) return tasks.First();

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

        public void StartReceiving()
        {
            _botClient.StartReceiving(
                new DefaultUpdateHandler(DefaultHandleUpdateAsync, DefaultHandleErrorAsync), 
                _cancellationToken.Token
                );
        }

        public void StopReceiving()
        {
            _cancellationToken.Cancel();
        }

        public void RestartReceiving()
        {
            StopReceiving();
            StartReceiving();
        }

        #region Comments
        public void AddResponse(
            Func<Message, bool> filterFunc = null,
            List<string> commandsList = null,
            Func<Update, Task> response = null
            )
        {
            Task NewResponse(Update update)
            {
                if ((filterFunc == null || filterFunc(update.Message)) && (commandsList == null || commandsList.Contains(update.Message.Text)))
                {
                    return response(update);
                }

                return Task.Run(() => { });
            }

            _updateHandle += NewResponse;
        }

        public Func<Update, Task> AddQuery(
            string queryText,
            Func<Update, Task> response = null
            )
        {
            Task QueryTask(Update update)
            {
                ChatId chatId = update.Message.Chat.Id;

                Task[] tasks =
                {
                    _botClient.SendTextMessageAsync(chatId: chatId, text: queryText),
                    Task.Run(() => _queriedChatIds.Add(chatId.Identifier, response.Invoke))
                };

                return Task.WhenAll(tasks);
            }

            return QueryTask;
        }
        #endregion
    }
}
