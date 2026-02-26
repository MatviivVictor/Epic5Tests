namespace Epic5Task.Application.Exceptions;

public class AuthZException : Exception
{
    public AuthZException()
    {
    }

    public AuthZException(string message) : base(message)
    {
    }

    public AuthZException(string message, Exception innerException) : base(message, innerException)
    {
    }
}