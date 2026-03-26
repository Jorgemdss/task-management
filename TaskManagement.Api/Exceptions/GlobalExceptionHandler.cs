using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            BaseDomainException ex => (ex.StatusCode, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            status = (int)statusCode,
            message = message,
            timestamp = DateTime.UtcNow
        }, cancellationToken);


        return true;
    }
}