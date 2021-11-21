using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class Query
    {
        public bool IsQueried { get; private set; }
        public Func<Task> Response { get; }
    }
}
