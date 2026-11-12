using Microsoft.AspNetCore.Diagnostics;
using SongTheSame.Domain.Game;

namespace SongTheSame.Api.Setup;

/// <summary>Maps domain and application exceptions to proper HTTP problem responses.</summary>
public static class ErrorHandling
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services) =>
        services.AddProblemDetails().AddExceptionHandler<ApplicationExceptionHandler>();

    private sealed class ApplicationExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var status = exception switch
            {
                GameRuleException => StatusCodes.Status409Conflict,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => 0,
            };
            if (status == 0) return false;

            context.Response.StatusCode = status;
            return await problems.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = { Status = status, Title = exception.Message },
            });
        }
    }
}
