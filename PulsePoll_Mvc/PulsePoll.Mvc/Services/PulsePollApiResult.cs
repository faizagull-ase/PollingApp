namespace PulsePoll.Mvc.Services;

public class PulsePollApiResult<T>
{
    public bool Success { get; }
    public int StatusCode { get; }
    public T? Data { get; }
    public string? Error { get; }

    private PulsePollApiResult(bool success, int statusCode, T? data, string? error)
    {
        Success = success;
        StatusCode = statusCode;
        Data = data;
        Error = error;
    }

    public static PulsePollApiResult<T> Ok(int statusCode, T data) => new(true, statusCode, data, null);

    public static PulsePollApiResult<T> Fail(int statusCode, string error) => new(false, statusCode, default, error);
}
