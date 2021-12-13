using Telegram.Bot;
using System;
using TelegramBot;

namespace QueueBot.Commands
{
    class SendMessageCommand : ICommand
    {
        private string _text;
        private IBot _bot;

        public SendMessageCommand(string text, IBot bot)
        {
            _text = text;
            _bot = bot;
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
