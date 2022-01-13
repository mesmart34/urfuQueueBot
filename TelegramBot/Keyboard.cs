using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;

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

        private readonly List<string> _buttonsNames;

        public Keyboard(List<string> buttonsNames)
        {
            _buttonsNames = buttonsNames;
        }

        public Keyboard(params string[] buttonsNames)
        {
            _buttonsNames = buttonsNames.ToList();
        }

        public IReplyMarkup GetReplyMarkup()
        {
            return new ReplyKeyboardMarkup(GetKeyboardButtons());
        }

        private List<KeyboardButton[]> GetKeyboardButtons()
        {
            List<KeyboardButton[]> buttons = new List<KeyboardButton[]>();
            for (int i = 0; i < _buttonsNames.Count; ++i)
            {
                KeyboardButton[] button = {
                    GetKeyboardButton(_buttonsNames[i])
                };
                buttons.Add(button);
            }

            return buttons;
        }

        private KeyboardButton GetKeyboardButton(string text)
        {
            return new KeyboardButton(text);
        }
    }
}
