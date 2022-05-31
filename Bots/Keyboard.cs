using System.Collections.Generic;
using System.Linq;

namespace Bots
{
    public class Keyboard
    {
        protected readonly List<string> _buttonsNames;

        public Keyboard(params string[] buttonsNames)
        {
            _buttonsNames = buttonsNames.ToList();
        }
    }
}
