namespace UMS.Runtime.Core
{
    public interface IModEntry
    {
        string Extension { get; }
        string FileName { get; }
        string FolderName { get; }
    }
}