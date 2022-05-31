using System.Threading.Tasks;

namespace Bots.UpdateHandlers
{
    public interface IUpdateHandler
    {
        Task InvokeMessage(IBot bot, long chatId, string message);

        public void AddResponse(Response response);
        void RemoveResponse(Response response);

        void AddQuery(long chatId, Response.ResponseFunc response);
    }
}
