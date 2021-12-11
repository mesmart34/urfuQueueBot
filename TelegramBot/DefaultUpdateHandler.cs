using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;

namespace TelegramBot
{
    public class DefaultUpdateHandler : IUpdateHandler
    {
        private Func<Update, Task> _updateHandle;
        private Dictionary<ChatId, Func<Update, Task>> _queriedChatIds;

        public DefaultUpdateHandler()
        {
            _queriedChatIds = new Dictionary<ChatId, Func<Update, Task>>();
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
            // clear old updates
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
                _queriedChatIds[chatId](update);
                _queriedChatIds.Remove(chatId);
            }
        }

        public void AddResponse(
            Func<Message, bool> filter = null,
            List<string> commandsList = null,
            Func<Update, Task> response = null
            )
        {
            Task NewResponse(Update update)
            {
                if ((filter == null || filter(update.Message)) && (commandsList == null || commandsList.Contains(update.Message.Text)))
                {
                    return response(update);
                }
                return Task.Run(() => { });
                // return Task.CompletedTask;
            }

            _updateHandle += NewResponse;
        }

        public Func<Update, Task> GetQuery(
            IBot bot,
            string queryText,
            Func<Update, Task> response = null
            )
        {
            Task QueryTask(Update update)
            {
                ChatId chatId = update.Message.Chat.Id;

                Task[] tasks =
                {
                    bot.SendMessage(chatId: chatId, text: queryText),
                    Task.Run(() => _queriedChatIds.Add(chatId.Identifier, response.Invoke))
                };

                return Task.WhenAll(tasks);
            }

            return QueryTask;
        }
    }
}
