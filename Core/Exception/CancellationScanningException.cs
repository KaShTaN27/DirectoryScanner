namespace Core.Exception;

public class CancellationScanningException : System.Exception
{
    public CancellationScanningException(string? message) : base(message)
    {
    }
}