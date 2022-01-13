using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Logger;
using static TelegramBot.UpdateHandler.Response;

namespace TelegramBot.UpdateHandler
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        private ResponseFunc _updateHandler;
        private Dictionary<ChatId, Queue<ResponseFunc>> _queriedChatIds;

        private readonly ILogger _logger;

        private bool _toClearUpdates;
        private readonly DateTime _startTime;

        public DefaultUpdateHandler()
        {
            _queriedChatIds = new Dictionary<ChatId, Queue<ResponseFunc>>();
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

            await Task.Run(() => Console.WriteLine(errorMessage));
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

            if (!_queriedChatIds.ContainsKey(chatId) || _queriedChatIds[chatId].Count == 0)
            {
                await Task.Run(() => { _updateHandler(update); });
            }
            else
            {
                await Task.Run(() => { _queriedChatIds[chatId].Dequeue()(update); });
            }
        }

        public Task InvokeMessage(ITelegramBotClient botClient, Message message)
        {
            var upd = new Update();
            upd.Message = message;
            return HandleUpdateAsync(botClient, upd, CancellationToken.None);
        }

        public void AddResponse(Response response)
        {
            _updateHandler += response.GetWrappedResponse();
        }

        public void RemoveResponse(Response response)
        {
            _updateHandler -= response.GetWrappedResponse();
        }

        public void AddQuery(Update upd, ResponseFunc response)
        {
            if (!_queriedChatIds.ContainsKey(upd.Message.Chat.Id))
                _queriedChatIds.Add(upd.Message.Chat.Id, new Queue<ResponseFunc>());

            _queriedChatIds[upd.Message.Chat.Id].Enqueue(response);
        }
    }
}
