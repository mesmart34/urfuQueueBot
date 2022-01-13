using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TableParser;
using SendableFiles;
using Telegram.Bot.Types.InputFiles;
using TelegramBot.UpdateHandler;

namespace TelegramBot
{
    public class Bot : IBot
    {
        protected readonly TelegramBotClient _botClient;
        private readonly CancellationTokenSource _cancellationToken;

        public BotData Data { get; }

        public IUpdateHandler UpdateHandler { get; }

        public Bot(BotToken token, IUpdateHandler updateHandler, DataBase db)
        {
            Data = new BotData(db);
            _botClient = new TelegramBotClient(token.GetToken());
            _cancellationToken = new CancellationTokenSource();
            UpdateHandler = updateHandler;
        }

        public async void SendMessageAsync(
            ChatId chatId, 
            string text = null, 
            IEnumerable<ISendable> content = null, 
            Keyboard keyboard = null
        )
        {
            if (text != null)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: keyboard?.GetReplyMarkup() ?? Keyboard.RemoveMarkup
                );
            }

            if (content == null) return;

            foreach (var file in content)
            {
                await file.Send(_botClient, chatId);
            }
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
                UpdateHandler,
                new Telegram.Bot.Extensions.Polling.ReceiverOptions { AllowedUpdates = { } },
                _cancellationToken.Token
                );
        }

        public void StopReceiving()
        {
            _cancellationToken.Cancel();
        }

        public void InvokeMessage(Message mes)
        {
            UpdateHandler.InvokeMessage(_botClient, mes);
        }
    }
}
