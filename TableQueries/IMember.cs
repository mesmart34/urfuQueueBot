using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableQueries
{
    // TODO: Реализовать IMember

    public interface IMember
    {
        string Name { get; }
        NotificationType Notification { get; }
    }
}
