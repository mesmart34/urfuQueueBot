using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableQueries;

namespace TableParser
{
    // TODO: Реализовать IMember

    public interface IMember
    {
        string Name { get; }
        NotificationType Notification { get; }

        void Notify();
    }
}
