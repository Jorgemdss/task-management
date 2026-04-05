using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger _logger;
    private readonly IProblemDetailsService _problemDetailService;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger
    )
    {
        _logger = logger;
        _problemDetailService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            BaseDomainException ex => (ex.StatusCode, ex.Message),
            _ => (HttpStatusCode.InternalServerError, exception.Message),
        };

        httpContext.Response.StatusCode = (int)statusCode;

        return await _problemDetailService.TryWriteAsync(
            new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occured",
                    Detail = message,
                },
            }
        );
    }
}
