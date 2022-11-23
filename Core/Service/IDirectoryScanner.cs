using Core.Model;

namespace Core.Service;

public interface IDirectoryScanner
{
    public FileSystemObject Scan(string path);
    
    public void StopScanning();

    public bool IsScanning();
}