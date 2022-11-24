using Core.Exception;
using Core.Model;
using Core.Service;

namespace Tests;

public class DirectoryScannerTest {
    private const string NonExistentDirectoryPath = @"C:\NonExistentDirectory";
    private const string CorrectDirectoryPath = @"D:\Games";

    private readonly DirectoryScanner _directoryScanner = new();

    [SetUp]
    public void Setup() {
    }

    [Test]
    public void Test_IfDirectoryDoesNotExist() {
        Assert.Throws<InvalidPathException>(() => _directoryScanner.Scan(NonExistentDirectoryPath));
    }

    [Test]
    public void Test_IfDirectoryPathCorrect() {
        var fileSystemObject = _directoryScanner.Scan(CorrectDirectoryPath);
        fileSystemObject.ComputeStatistics();
        Assert.Multiple(() => {
            Assert.That(fileSystemObject.Childs, Has.Count.EqualTo(4));
            Assert.That(fileSystemObject.Size, Is.EqualTo(234970386662));
        });
    }

    [Test]
    public void Test_IfInterruptScanning() {
        var fileSystemObject = _directoryScanner.Scan(CorrectDirectoryPath);
        fileSystemObject.ComputeStatistics();

        FileSystemObject interruptedFileSystemObject = null!;
        var task = Task.Run(() => 
            interruptedFileSystemObject = _directoryScanner.Scan(CorrectDirectoryPath));
        Thread.Sleep(50);
        _directoryScanner.StopScanning();
        task.Wait();
        
        interruptedFileSystemObject.ComputeStatistics();
        Assert.That(fileSystemObject.Size, Is.GreaterThan(interruptedFileSystemObject.Size));
    }
}