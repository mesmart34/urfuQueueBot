using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Loggers;

using Bots.VK;

using ResponseFunc = Bots.UpdateHandlers.Response.ResponseFunc;

namespace Bots.UpdateHandlers.VK
{
    public class VKUpdateHandler : IUpdateHandler
    {
        private ResponseFunc _updateHandler;

        // map[ chatId, queue< respFunc > ]
        private Dictionary<long, Queue<ResponseFunc>> _queriedChatIds;

        private readonly Logger _logger;

        private bool _toClearUpdates;
        private readonly DateTime _startTime;

        public VKUpdateHandler()
        {
            _queriedChatIds = new Dictionary<long, Queue<ResponseFunc>>();
            _toClearUpdates = true;
            _startTime = DateTime.UtcNow;

            // TODO : add path / folder to resources
            _logger = new Logger("../../../../logs/");
        }

        public async Task HandleUpdateAsync(InputMessage im)
        {
            if (_toClearUpdates)
            {
                if (im.Time < _startTime)
                {
                    string skipText =
                        $"[ {im.Time:HH:mm:ss} ] : Skip a message '{im.Text}' in chat @{im.SenderId} [{im.Username}].";
                    _logger.Log(skipText);
                    Console.WriteLine(skipText);
                    return;
                }

                _toClearUpdates = false;
            }

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
            // Такой проверки не должно быть
            if (bot is VKBot vkbot)
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

        // TODO: Вынести в абстрактный(?) класс
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
