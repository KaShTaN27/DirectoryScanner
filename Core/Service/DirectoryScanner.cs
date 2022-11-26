using System.Collections.Concurrent;
using Core.Exception;
using Core.Model;

namespace Core.Service;

public class DirectoryScanner : IDirectoryScanner {
    private SemaphoreSlim _semaphoreSlim = new(50);
    private CancellationTokenSource _tokenSource = new();
    private ConcurrentQueue<FileSystemObject> _directoriesQueue = new();
    private bool _isScanning;

    public FileSystemObject Scan(string path) {
        return File.Exists(path) ? ProcessFile(path) : ProcessDirectory(path);
    }

    public void StopScanning() {
        if (!_isScanning)
            throw new CancellationScanningException("Scanning is already stopped");
        _isScanning = false;
        _tokenSource.Cancel();
    }

    public bool IsScanning() {
        return _isScanning;
    }

    private FileSystemObject ProcessFile(string path) {
        var fileInfo = new FileInfo(path);
        return new FileSystemObject(fileInfo.FullName, fileInfo.Name, fileInfo.Length);
    }

    private FileSystemObject ProcessDirectory(string path) {
        var directoryInfo = GetDirectoryInfo(path);
        var sourceToken = _tokenSource.Token;
        return StartScanning(directoryInfo, sourceToken);
    }

    private DirectoryInfo GetDirectoryInfo(string path) {
        var directorInfo = new DirectoryInfo(path);
        return directorInfo.Exists ? directorInfo : throw new InvalidPathException("Invalid directory path");
    }

    private FileSystemObject StartScanning(DirectoryInfo directoryInfo, CancellationToken token) {
        _isScanning = true;
        var fileSystemDirectory = new FileSystemObject(directoryInfo.FullName, directoryInfo.Name);
        ScanDirectory(fileSystemDirectory, token);
        ScanAllSubdirectories(token);
        _isScanning = false;
        return fileSystemDirectory;
    }

    private void ScanDirectory(FileSystemObject fileSystemDirectory, CancellationToken sourceToken) {
        var directoryInfo = new DirectoryInfo(fileSystemDirectory.Path);
        ScanSubdirectories(fileSystemDirectory, directoryInfo, sourceToken);
        ScanFiles(fileSystemDirectory, directoryInfo, sourceToken);
    }

    private void ScanAllSubdirectories(CancellationToken token) {
        do {
            if (_directoriesQueue.TryDequeue(out var subdirectory))
                ScanSubdirectoryInNewThread(subdirectory, token);
        } while ((_semaphoreSlim.CurrentCount != 50 || !_directoriesQueue.IsEmpty) &&
                 !token.IsCancellationRequested);
    }

    private void ScanSubdirectoryInNewThread(FileSystemObject subdirectory, CancellationToken sourceToken) {
        try {
            _semaphoreSlim.Wait(sourceToken);
            Task.Run(() => {
                ScanDirectory(subdirectory, sourceToken);
                _semaphoreSlim.Release();
            }, sourceToken);
        }
        catch (System.Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    private void ScanSubdirectories(FileSystemObject fileSystemDirectory, DirectoryInfo directoryInfo,
        CancellationToken sourceToken) {
        var subdirectories = GetSubdirectories(directoryInfo);
        foreach (var subdirectory in subdirectories) {
            if (sourceToken.IsCancellationRequested)
                break;
            var fileSystemSubdirectory = new FileSystemObject(subdirectory.FullName, subdirectory.Name);
            fileSystemDirectory.Childs.Add(fileSystemSubdirectory);
            _directoriesQueue.Enqueue(fileSystemSubdirectory);
        }
    }

    private DirectoryInfo[] GetSubdirectories(DirectoryInfo directoryInfo) {
        try {
            return directoryInfo.GetDirectories().Where(info => info.LinkTarget == null).ToArray();
        }
        catch (SystemException e) {
            Console.WriteLine(e.Message);
            return Array.Empty<DirectoryInfo>();
        }
    }

    private void ScanFiles(FileSystemObject fileSystemDirectory, DirectoryInfo directoryInfo,
        CancellationToken sourceToken) {
        var filesInfo = GetFiles(directoryInfo);
        foreach (var fileInfo in filesInfo) {
            if (sourceToken.IsCancellationRequested)
                break;
            fileSystemDirectory.Childs.Add(new FileSystemObject(fileInfo.FullName, fileInfo.Name, fileInfo.Length));
        }
    }

    private static FileInfo[] GetFiles(DirectoryInfo directoryInfo) {
        try {
            return directoryInfo.GetFiles().Where(info => info.LinkTarget == null).ToArray();
        }
        catch (System.Exception e) {
            Console.WriteLine(e.Message);
            return Array.Empty<FileInfo>();
        }
    }
}