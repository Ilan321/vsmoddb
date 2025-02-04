namespace VsModDb.Services.Middleware;

public class RequestIdMiddleware(ILogger<RequestIdMiddleware> log) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestId = context.TraceIdentifier;

        using var scope = log.BeginScope(
            new Dictionary<string, object>
            {
                ["RequestId"] = requestId
            }
        );

        context.Response.Headers.Append("Request-Id", requestId);

        await next(context);
    }
}
