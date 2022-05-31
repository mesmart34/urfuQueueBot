using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bots.TG
{
    public class Keyboard : Bots.Keyboard
    {
        public static IReplyMarkup RemoveMarkup
        {
            get
            {
                return new ReplyKeyboardRemove();
            }
        }

        public IReplyMarkup GetReplyMarkup()
        {
            return new ReplyKeyboardMarkup(GetKeyboardButtons());
        }

        public Keyboard(params string[] buttonsNames) : base(buttonsNames) { }

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
