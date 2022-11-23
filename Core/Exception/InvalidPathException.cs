namespace Core.Exception;

public class InvalidPathException : System.Exception
{
    public InvalidPathException(string? message) : base(message)
    {
    }
}