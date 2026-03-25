using CourierTrack.Domain.Common;

namespace CourierTrack.API.Middlewares;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BaseException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailResult(ex.Message, ex.ErrorCode);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.FailResult(
                ex.Message,
                code: "INTERNAL_ERROR",
                details: new List<string> { ex.StackTrace ?? "No stack trace available" }
            );

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
