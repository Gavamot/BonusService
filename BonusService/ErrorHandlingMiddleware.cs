using System.Net;

namespace BonusService;

public class ErrorHandlingMiddleware : Exception
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var msg = $"{exception.GetType()}\n {exception.Message}\n {exception.InnerException?.Message}\n {exception.StackTrace}";
            _logger.LogError(exception, msg);
            await context.Response.WriteAsJsonAsync(msg);
        }
    }
}