using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Logger;

namespace TelegramBot
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        private Func<Update, Task> _updateHandle;
        private Dictionary<ChatId, Func<Update, Task>> _queriedChatIds;

        private ILogger _logger;

        private bool _toClearUpdates;
        private DateTime _startTime;

        public DefaultUpdateHandler()
        {
            _queriedChatIds = new Dictionary<ChatId, Func<Update, Task>>();
            _toClearUpdates = true;
            _startTime = DateTime.UtcNow;

            _logger = new Logger.Logger("../../../../logs/");
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (_toClearUpdates)
            {
                if (update.Message.Date < _startTime)
                {
                    string skipText = 
                        $"[ {update.Message.Date:HH:mm:ss} ] : Skip a message '{update.Message.Text}' in chat @{update.Message.From.Username} [{update.Message.Chat.Id}].";
                    _logger.Log(skipText);
                    Console.WriteLine(skipText);
                    return;
                }

                _toClearUpdates = false;
            }
            

            if (update.Type != UpdateType.Message)
                return;
            if (update.Message.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var message = update.Message.Text;

            string text = $"[ {update.Message.Date:HH:mm:ss} ] : Received a message '{update.Message.Text}' in chat @{update.Message.From.Username} [{chatId}].";
            _logger.Log(text);
            Console.WriteLine(text);

            if (!_queriedChatIds.ContainsKey(chatId))
            {
                _updateHandle(update);
            }
            else
            {
                _queriedChatIds[chatId](update);
                _queriedChatIds.Remove(chatId);
            }
        }

        public Task InvokeMessage(ITelegramBotClient botClient, Message message)
        {
            var upd = new Update();
            upd.Message = message;
            return HandleUpdateAsync(botClient, upd, CancellationToken.None);
        }

        public void AddResponse(
            Func<Message, bool> filter = null,
            Func<Update, Task> response = null
            )
        {
            Task NewResponse(Update update)
            {
                if (filter == null || filter(update.Message))
                {
                    return response(update);
                }
                return Task.Run(() => { });
                // return Task.CompletedTask;
            }

            _updateHandle += NewResponse;
        }

        public Func<Update, Task> GetQuery(
            Func<Update, Task> response = null
            )
        {
            Task QueryTask(Update update)
            {
                ChatId chatId = update.Message.Chat.Id;

                Task[] tasks =
                {
                    Task.Run(() => _queriedChatIds.Add(chatId.Identifier, response.Invoke))
                };

                return Task.WhenAll(tasks);
            }

            return QueryTask;
        }
    }
}
