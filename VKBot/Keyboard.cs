using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;

namespace Bots.VK
{
    public class Keyboard : Bots.Keyboard
    {
        public static MessageKeyboard RemoveKeyboard
        {
            get
            {
                return new KeyboardBuilder().Build();
            }
        }

        private KeyboardBuilder _keyboardBuilder;

        public Keyboard(params string[] buttonsNames) : base(buttonsNames)
        {
            _keyboardBuilder = new KeyboardBuilder();
        }

        public MessageKeyboard GetKeyboard()
        {
            foreach (var btn in _buttonsNames)
            {
                _keyboardBuilder.AddButton(btn, $"{btn}_btn", KeyboardButtonColor.Primary);
                _keyboardBuilder.AddLine();
            }

            return _keyboardBuilder.Build();
        }
    }
}
