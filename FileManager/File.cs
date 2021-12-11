namespace FileManager
{
    // TODO: Sticker
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
