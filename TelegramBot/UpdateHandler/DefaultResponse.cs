namespace TelegramBot.UpdateHandler
{
    public class DefaultResponse : Response
    {
        public override FilterFunc Filter { get; }

        public override ResponseFunc UpdateHandler { get; }

        public DefaultResponse(FilterFunc filter, ResponseFunc updateHandler)
        {
            Filter = filter;
            UpdateHandler = updateHandler;
        }
    }
}
