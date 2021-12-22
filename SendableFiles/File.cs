using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using System.Linq;

namespace SendableFiles
{
    public class Document : ISendable
    {
        private readonly string _path;
        private readonly string _caption;

        private string Name => _path.Split('/').Last();

        public Document(string path, string caption)
        {
            _path = path;
            _caption = caption;
        }

        public override Task Send(ITelegramBotClient client, ChatId chatId)
        {
            Stream s = System.IO.File.OpenRead(_path);
            return client.SendDocumentAsync(chatId, new InputOnlineFile(s, Name), caption: _caption);
        }
    }

    public class Image : ISendable
    {
        private readonly string _path;
        private readonly string _caption;

        public Image(string path, string caption)
        {
            _path = path;
            _caption = caption;
        }

        public override Task Send(ITelegramBotClient client, ChatId chatId)
        {
            Stream s = System.IO.File.OpenRead(_path);
            return client.SendPhotoAsync(chatId, new InputOnlineFile(s), caption: _caption);
        }
    }

    public class Sticker : ISendable
    {
        private readonly string _id;

        public Sticker(string id)
        {
            _id = id;
        }

        public override Task Send(ITelegramBotClient client, ChatId chatId)
        {
            return client.SendStickerAsync(chatId, new InputTelegramFile(_id).FileId);
        }
    }
}
