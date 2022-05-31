namespace Bots.UpdateHandlers
{
    public class Response
    {
        public delegate bool FilterFunc(InputMessage im);
        public delegate void ResponseFunc(InputMessage im);

        public FilterFunc Filter { get; }
        public ResponseFunc UpdateHandler { get; }

        public Response(FilterFunc filter, ResponseFunc updateHandler)
        {
            Filter = filter;
            UpdateHandler = updateHandler;
        }

        public ResponseFunc GetWrappedResponse()
        {
            return (InputMessage im) =>
            {
                if (Filter == null || Filter(im))
                {
                    UpdateHandler(im);
                }
            };
        }
    }
}
