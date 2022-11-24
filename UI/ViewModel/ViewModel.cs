using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Service;
using UI.Command;
using UI.Model;

namespace UI.ViewModel;

public class ViewModel : INotifyPropertyChanged {
    public RelayCommand Scan { get; set; }
    public RelayCommand StopScanning { get; set; }
    public RelayCommand ChooseDirectory { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private DirectoryScanner _directoryScanner;

    private FileSystemMember _fileSystemMember;

    public FileSystemMember FileSystemMember {
        get => _fileSystemMember;
        private set {
            _fileSystemMember = value;
            OnPropertyChanged();
        }
    }

    private string _path;

    public string Path {
        get => _path;
        private set {
            _path = value;
            OnPropertyChanged();
        }
    }

    private bool _isScanning;

    public bool IsScanning {
        get => _isScanning;
        set {
            _isScanning = value;
            OnPropertyChanged();
        }
    }

    public ViewModel() {
        Scan = new RelayCommand(_ => {
            Task.Run(() => {
                IsScanning = true;
                _directoryScanner = new DirectoryScanner();
                var fileSystemObject = _directoryScanner.Scan(Path);
                fileSystemObject.ComputeStatistics();
                FileSystemMember = new FileSystemMember(fileSystemObject);
                IsScanning = false;
            });
        }, _ => Path != null && !IsScanning);

        StopScanning = new RelayCommand(_ => {
            _directoryScanner!.StopScanning();
            IsScanning = false;
        }, _ => IsScanning);

        ChooseDirectory = new RelayCommand(_ => {
            using var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                Path = folderBrowserDialog.SelectedPath;
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}