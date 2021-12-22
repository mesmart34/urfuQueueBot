using Telegram.Bot;
using System;
using TelegramBot;
using Telegram.Bot.Types;
using System.Collections.Generic;
using SendableFiles;

namespace QueueBot.Commands
{
    class SendMessageCommand : ICommand
    {
        public IBot Bot { get; }
        private ChatId ChatId { get; set; }
        private string Text { get; set; }
        private IEnumerable<ISendable> Content { get; }
        private Keyboard Keyboard { get; set; }

        public SendMessageCommand(IBot bot) 
        {
            Bot = bot;
        }

        public SendMessageCommand(
            IBot bot,
            ChatId chatId,
            string text, 
            IEnumerable<ISendable> content,
            Keyboard keyboard)
        {
            Bot = bot;
            ChatId = chatId;
            Text = text;
            Content = content;
            Keyboard = keyboard;
        }

        public void Execute()
        {
            Bot.SendMessageTest(ChatId, Text, Content, Keyboard);
        }
    }
}
