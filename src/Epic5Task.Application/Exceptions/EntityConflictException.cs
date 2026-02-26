namespace Epic5Task.Application.Exceptions;

public class EntityConflictException : Exception
{
    public EntityConflictException()
    {
    }

    public EntityConflictException(string message) : base(message)
    {
    }

    public EntityConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}