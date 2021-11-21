using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FileManager
{
    public enum FileType
    {
        Document,
        Image,
        Audio,
        Video
    }

    public class File : IFile
    {
        public FileType FileType { get; }
        public string Caption { get; }
        public string Path { get; }

        public File(FileType fileType, string caption, string path)
        {
            FileType = fileType;
            Caption = caption;
            Path = path;
        }
    }
}
