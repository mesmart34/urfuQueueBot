using Telegram.Bot.Types;

namespace TelegramBot.UpdateHandler
{
    public abstract class Response
    {
        public delegate bool FilterFunc(Message mes);
        public delegate void ResponseFunc(Update upd);

        public abstract FilterFunc Filter { get; }
        public abstract ResponseFunc UpdateHandler { get; }

        public ResponseFunc GetWrappedResponse()
        {
            return (Update update) =>
            {
                if (Filter == null || Filter(update.Message))
                {
                    UpdateHandler(update);
                }
            };
        }
    }
}
