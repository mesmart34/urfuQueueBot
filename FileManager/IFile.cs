using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FileManager
{
    public interface IFile
    {
        FileType FileType { get; }
        string Caption { get; }
        string Path { get; }
    }
}
