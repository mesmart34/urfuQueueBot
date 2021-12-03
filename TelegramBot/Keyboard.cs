using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Keyboard
    {
        public static IReplyMarkup RemoveMarkup
        {
            get
            {
                return new ReplyKeyboardRemove();
            }
        }

        private List<string> _buttonsNames;

        public Keyboard(List<string> buttonsNames)
        {
            _buttonsNames = buttonsNames;
        }

        public IReplyMarkup GetReplyMarkup()
        {
            return new ReplyKeyboardMarkup(GetKeyboardButtons());
        }

        private KeyboardButton[] GetKeyboardButtons()
        {
            KeyboardButton[] buttons = new KeyboardButton[_buttonsNames.Count];
            for (int i = 0; i < _buttonsNames.Count; ++i)
            {
                buttons[i] = GetKeyboardButton(_buttonsNames[i]);
            }

            return buttons;
        }

        private KeyboardButton GetKeyboardButton(string text)
        {
            return new KeyboardButton(text);
        }
    }
}
