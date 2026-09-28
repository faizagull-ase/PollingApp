namespace PulsePoll.Exceptions;

public abstract class ApiException : Exception
{
    public abstract int StatusCode { get; }

    protected ApiException(string message) : base(message)
    {
    }
}

public class ValidationException : ApiException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;

    public ValidationException(string message) : base(message)
    {
    }
}

public class NotFoundException : ApiException
{
    public override int StatusCode => StatusCodes.Status404NotFound;

    public NotFoundException(string message) : base(message)
    {
    }
}
