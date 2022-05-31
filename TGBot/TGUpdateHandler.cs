using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Loggers;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;

using Bots.TG;

using ResponseFunc = Bots.UpdateHandlers.Response.ResponseFunc;

namespace Bots.UpdateHandlers.TG
{
    public class TGUpdateHandler : IUpdateHandler, Telegram.Bot.Extensions.Polling.IUpdateHandler
    {
        private ResponseFunc _updateHandler;

        // map[ chatId, queue< respFunc > ]
        private Dictionary<long, Queue<ResponseFunc>> _queriedChatIds;

        private readonly Logger _logger;

        private bool _toClearUpdates;
        private readonly DateTime _startTime;

        public TGUpdateHandler()
        {
            _queriedChatIds = new Dictionary<long, Queue<ResponseFunc>>();
            _toClearUpdates = true;
            _startTime = DateTime.UtcNow;

            // TODO : add path / folder to resources
            _logger = new Logger("../../../../logs/");
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
                if (update.Message == null || update.Message.Date < _startTime)
                {
                    string skipText =
                        $"[ {update.Message?.Date:HH:mm:ss} ] : Skip a message '{update.Message?.Text}' in chat @{update.Message?.From.Username} [{update.Message?.Chat.Id}].";
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

            InputMessage im = new InputMessage
            {
                SenderId = update.Message.Chat.Id,
                Text = update.Message.Text,
                Time = update.Message.Date,
                Username = update.Message.From.Username
            };

            await HandleInputMessage(im);
        }

        private async Task HandleInputMessage(InputMessage im)
        {
            string text = $"[ {im.Time:HH:mm:ss} ] : Received a message '{im.Text}' in chat @{im.Username} [{im.SenderId}].";
            _logger.Log(text);
            Console.WriteLine(text);

            if (!_queriedChatIds.ContainsKey(im.SenderId) || _queriedChatIds[im.SenderId].Count == 0)
            {
                await Task.Run(() => { _updateHandler(im); });
            }
            else
            {
                await Task.Run(() => { _queriedChatIds[im.SenderId].Dequeue()(im); });
            }
        }

        public Task InvokeMessage(IBot bot, long chatId, string message)
        {
            if (bot is TGBot tgBot)
            {
                InputMessage im = new InputMessage
                {
                    SenderId = chatId,
                    Text = message,
                    Time = DateTime.UtcNow,
                    Username = "SYSTEM / To"
                };

                return HandleInputMessage(im);
            }

            return new Task(() => { });
        }

        public void AddResponse(Response response)
        {
            _updateHandler += (InputMessage im) => 
            {
                if (response.Filter == null || response.Filter(im))
                {
                    response.UpdateHandler(im);
                }
            };
        }

        public void RemoveResponse(Response response)
        {
            _updateHandler -= (InputMessage im) =>
            {
                if (response.Filter == null || response.Filter(im))
                {
                    response.UpdateHandler(im);
                }
            };
        }

        public void AddQuery(long chatId, ResponseFunc response)
        {
            if (!_queriedChatIds.ContainsKey(chatId))
                _queriedChatIds.Add(chatId, new Queue<ResponseFunc>());

            _queriedChatIds[chatId].Enqueue(response);
        }
    }
}
