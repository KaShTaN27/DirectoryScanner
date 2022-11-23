namespace Core.Model;

public class FileSystemObject
{
    public string Path { get; }
    public string Name { get; }
    public float Percentage;
    public long Size;
    public IList<FileSystemObject> Childs = new List<FileSystemObject>();

    public FileSystemObject(string path, string name, long size)
    {
        Size = size;
        Path = path;
        Name = name;
    }

    public FileSystemObject(string path, string name)
    {
        Path = path;
        Name = name;
    }

    public void ComputeStatistics() {
        ComputeSize();
        ComputePercentage();
    }

    private long ComputeSize() {
        foreach (var child in Childs) {
            Size += child.ComputeSize();
        }
        return Size;
    }

    private void ComputePercentage() {
        foreach (var child in Childs) {
            child.Percentage = (float)child.Size / this.Size * 100;
            child.ComputePercentage();
        }
    }
}