using System.Collections.Generic;

namespace QueueBot
{
    class Invoker
    {
        Dictionary<string, ICommand> _commands;

        public Invoker() 
        {
            _commands = new Dictionary<string, ICommand>();
        }

        public void SetCommand(string key, ICommand command)
        {
            _commands[key] = command;
        } 

        public void ExecuteCommand(string key)
        {
            _commands[key].Execute();
        }
    }
}
