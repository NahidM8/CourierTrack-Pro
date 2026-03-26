using CourierTrack.Domain.Common;

namespace CourierTrack.API.Middlewares;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BaseException ex)
        {
            logger.LogWarning(ex, "Application exception occurred: {ErrorCode}", ex.ErrorCode);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An internal server error occurred.",
                "INTERNAL_ERROR"
            );
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string message,
        string errorCode)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.FailResult(message, code: errorCode);
        return context.Response.WriteAsJsonAsync(response);
    }
}
