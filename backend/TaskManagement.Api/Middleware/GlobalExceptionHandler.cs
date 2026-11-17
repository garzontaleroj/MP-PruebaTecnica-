using Microsoft.AspNetCore.Diagnostics;
using TaskManagement.Api.Errors;

namespace TaskManagement.Api.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ApiException apiException)
        {
            httpContext.Response.StatusCode = apiException.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(
                new ApiErrorResponse(apiException.Code, apiException.StatusCode, apiException.Message),
                cancellationToken);
            return true;
        }

        logger.LogError(exception, "An unexpected error occurred while processing the request.");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            }
        });
    }
}
