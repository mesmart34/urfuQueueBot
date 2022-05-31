using System.Collections.Generic;
using Bots.UpdateHandlers;

namespace Bots
{
    public interface IBot
    {
        Keyboard GetKeyboard(params string[] buttonsNames);
        void AddResponse(Response.FilterFunc filter, Response.ResponseFunc response);

        Data Data { get; }

        IUpdateHandler UpdateHandler { get; }

        void StartReceiving();
        void StopReceiving();

        // TODO : ...(unsigned int userId, string text, ienum<isend> content, keyboard kb)
        void SendMessageAsync(long chatId, string text, Keyboard keyboard = null);
        void InvokeMessage(long userId, string message);

        void SendPhotoAsync(long userId, string path, string caption, Keyboard keyboard = null);
        void SendDocumentAsync(long userId, string path, string caption, Keyboard keyboard = null);
        void SendStickerAsync(long userId, string set, uint index, Keyboard keyboard = null);
    }
}
