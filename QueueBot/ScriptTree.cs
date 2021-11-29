using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace QueueBot
{
    public class ScriptTree
    {
        public List<ScriptLayer> Layers { get; }

        private ScriptLayer _rootLayer => Layers.First();

        public ScriptTree()
        {
            Layers = new List<ScriptLayer>();
        }

        public ScriptTree(IEnumerable<ScriptLayer> layers)
        {
            Layers = new List<ScriptLayer>(layers);
        }
    }

    public class ScriptNode
    {
        public string Text { get; }
        public Func<Update, Task> Response { get; }
        public bool IsRoot { get; }
        public List<ScriptNode> NextNodes { get; }

        public ScriptNode(string text, Func<Update, Task> resp, bool isRoot, IEnumerable<ScriptNode> next)
        {
            Text = text;
            Response = resp;
            IsRoot = isRoot;
            NextNodes = new List<ScriptNode>(next);
        }
    }

    public class ScriptLayer
    {
        public List<ScriptNode> Nodes { get; }

        public ScriptLayer(IEnumerable<ScriptNode> nodes)
        {
            Nodes = new List<ScriptNode>(nodes);
        }
    }
}
