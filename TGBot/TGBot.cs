using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TableParser;
using Telegram.Bot.Types.InputFiles;
using System.IO;
//using SQLDB;

using Bots.UpdateHandlers;
using Bots.UpdateHandlers.TG;

namespace Bots.TG
{
    public class TGBot : IBot
    {
        public readonly TelegramBotClient BotClient;
        private readonly CancellationTokenSource _cancellationToken;

        private Dictionary<string, StickerSet> _stickerSets;

        public Data Data { get; }

        public IUpdateHandler UpdateHandler { get; }

        public TGBot(Token token, IUpdateHandler updateHandler, Data data)
        {
            //Data = new BotData(db);
            Data = data;
            BotClient = new TelegramBotClient(token.GetToken());
            _cancellationToken = new CancellationTokenSource();
            UpdateHandler = updateHandler;
            _stickerSets = new Dictionary<string, StickerSet>();
        }

        public void LoadStickerSet(string key, string name)
        {
            _stickerSets.Add(key, BotClient.GetStickerSetAsync(name).Result);
        }

        public async void SendMessageAsync(
            long chatId,
            string text,
            Bots.Keyboard keyboard = null
        )
        {
            if (keyboard is not Keyboard && keyboard != null)
                throw new Exception("Not-TG keyboard using for TG bot");

            await BotClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: text,
                        replyMarkup: (keyboard as Keyboard)?.GetReplyMarkup() ?? Keyboard.RemoveMarkup
                    );
        }

        public Task SendSticker(ChatId chatId, string stId)
        {
            var st = new InputTelegramFile(stId);
            return BotClient.SendStickerAsync(chatId, st.FileId);
        }

        public StickerSet GetStickerSet(string shortName)
        {
            return BotClient.GetStickerSetAsync(shortName).Result;
        }

        public void StartReceiving()
        {
            BotClient.StartReceiving(
                UpdateHandler as TGUpdateHandler, 
                new Telegram.Bot.Extensions.Polling.ReceiverOptions { AllowedUpdates = { } },
                _cancellationToken.Token
                );
        }

        public void StopReceiving()
        {
            _cancellationToken.Cancel();
        }

        public void InvokeMessage(long userId, string mes)
        {
            UpdateHandler.InvokeMessage(this, userId, mes);
        }

        public async void SendPhotoAsync(long userId, string path, string caption, Bots.Keyboard keyboard = null)
        {
            if (keyboard is not Keyboard && keyboard != null)
                    throw new Exception("Non-TG keyboard attached to TG bot");

            Stream s = System.IO.File.OpenRead(path);
            await BotClient.SendPhotoAsync(userId, new InputOnlineFile(s), caption: caption, replyMarkup: (keyboard as Keyboard)?.GetReplyMarkup());
        }

        public async void SendDocumentAsync(long userId, string path, string caption, Bots.Keyboard keyboard = null)
        {
            if (keyboard is not Keyboard && keyboard != null)
                throw new Exception("Non-TG keyboard attached to TG bot");

            Stream s = System.IO.File.OpenRead(path);
            await BotClient.SendDocumentAsync(userId, new InputOnlineFile(s), caption: caption, replyMarkup: (keyboard as Keyboard)?.GetReplyMarkup());
        }

        public async void SendStickerAsync(long userId, string set, uint index, Bots.Keyboard keyboard = null)
        {
            await BotClient.SendStickerAsync(userId, new InputTelegramFile(_stickerSets[set].Stickers[index].FileId).FileId);
        }

        public void AddResponse(Response.FilterFunc filter, Response.ResponseFunc response)
        {
            UpdateHandler.AddResponse(new Response(filter, response));
        }

        public Bots.Keyboard GetKeyboard(params string[] buttonsNames)
        {
            return new Keyboard(buttonsNames);
        }
    }
}
