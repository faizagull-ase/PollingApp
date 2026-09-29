using Microsoft.AspNetCore.Http;

namespace PulsePoll.Models.Common;

public class ServiceResult<T>
{
    public bool Success { get; private init; }
    public int StatusCode { get; private init; }
    public T? Data { get; private init; }
    public string? Error { get; private init; }

    public static ServiceResult<T> Ok(T data, int statusCode = StatusCodes.Status200OK) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Data = data
    };

    public static ServiceResult<T> Fail(string error, int statusCode) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Error = error
    };
}
