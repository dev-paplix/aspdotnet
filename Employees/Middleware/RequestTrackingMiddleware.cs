namespace Employees.Middleware;

using Employees.Controllers.MVC;


public class RequestTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTrackingMiddleware> _logger;

    public RequestTrackingMiddleware (RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationStatesService appState)
    {
        // Increament total request
        appState.IncrementRequests();

        //log request details
        _logger.LogInformation(
            "Request: {Method} {Path} from {IP}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        // Track request timing
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Call the next middleware in the pipline
        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "Response: {StatusCode} in {ElapsedMS}ms",
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }

}

public static class RequestTrackingMiddlewareExtentions
{
    public static IApplicationBuilder UseRequestTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTrackingMiddleware>();
    }
}