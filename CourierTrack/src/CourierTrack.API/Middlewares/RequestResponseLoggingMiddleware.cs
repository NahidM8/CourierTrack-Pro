using System.Diagnostics;

namespace CourierTrack.API.Middlewares;

public class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestBody = await ReadRequestBody(context.Request);

        logger.LogInformation(
            "HTTP Request: {Method} {Path} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            requestBody
        );

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);

            stopwatch.Stop();

            var responseText = await ReadResponseBody(context.Response);

            logger.LogInformation(
                "HTTP Response: {StatusCode} | Time: {Elapsed}ms | Body: {Body}",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                responseText
            );
        }
        finally
        {
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength == 0 || request.ContentLength is null)
            return string.Empty;

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private async Task<string> ReadResponseBody(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);

        var text = await new StreamReader(response.Body).ReadToEndAsync();

        response.Body.Seek(0, SeekOrigin.Begin);

        return text;
    }
}
