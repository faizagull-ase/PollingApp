using System.Net;
using System.Text.Json;
using PulsePoll.Exceptions;

namespace PulsePoll.Middleware;

// Maps validation/not-found/conflict exceptions to the spec's HTTP status table (section 7)
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, body) = Map(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        if (body is not null)
        {
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }

    private static (HttpStatusCode StatusCode, object? Body) Map(Exception exception) => exception switch
    {
        ValidationFailedException ex => (HttpStatusCode.BadRequest, new { error = "invalid_request", errors = ex.Errors }),
        ResourceNotFoundException ex => (HttpStatusCode.NotFound, new { error = "not_found", message = ex.Message }),
        BadHttpRequestException => (HttpStatusCode.BadRequest, new { error = "invalid_request", message = "Malformed request body." }),
        _ => (HttpStatusCode.InternalServerError, new { error = "internal_error", message = "An unexpected error occurred." })
    };
}
