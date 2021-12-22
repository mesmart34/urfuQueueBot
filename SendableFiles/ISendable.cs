using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SendableFiles
{
    public abstract class ISendable
    {
        public abstract Task Send(ITelegramBotClient client, ChatId id);

        public static implicit operator ISendable[](ISendable sendable)
        {
            return new ISendable[] { sendable };
        }
    }
}
