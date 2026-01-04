// Utils/GlobalExceptionHandler.cs
using Microsoft.AspNetCore.Diagnostics;

namespace Sebigy.Dialogisera.Api.Utils;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, logLevel) = exception switch
        {
            UnauthorizedAccessException => 
                (StatusCodes.Status403Forbidden, "Forbidden", LogLevel.Warning),
            InvalidOperationException => 
                (StatusCodes.Status401Unauthorized, "Unauthorized", LogLevel.Warning),
            ArgumentException => 
                (StatusCodes.Status400BadRequest, "Bad Request", LogLevel.Warning),
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Not Found", LogLevel.Information),
            _ => 
                (StatusCodes.Status500InternalServerError, "Internal Server Error", LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "Request failed: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode;
        
        // Create response based on environment
        if (_environment.IsDevelopment())
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231",
                title,
                status = statusCode,
                detail = exception.Message,
                traceId = httpContext.TraceIdentifier,
                exceptionType = exception.GetType().FullName,
                stackTrace = exception.StackTrace
            }, cancellationToken);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231",
                title,
                status = statusCode,
                detail = exception.Message,
                traceId = httpContext.TraceIdentifier
            }, cancellationToken);
        }

        return true;
    }
}