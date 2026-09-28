namespace PulsePoll.Exceptions;

// 400 — malformed request body / failed field validation (spec section 7)
public class ValidationFailedException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationFailedException(IReadOnlyList<string> errors)
        : base(errors.Count > 0 ? errors[0] : "Validation failed.")
    {
        Errors = errors;
    }

    public ValidationFailedException(string error) : this(new[] { error })
    {
    }
}

// 404 — resource not found (spec section 7)
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message)
    {
    }
}

