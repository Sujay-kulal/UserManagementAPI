namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Custom middleware that logs details about each HTTP request and response.
    /// Logs the HTTP method, request path, and response status code.
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes an HTTP request by logging its details before and after passing
        /// it to the next middleware in the pipeline.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            // Log the incoming request details
            _logger.LogInformation("Incoming Request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            // DEBUGGING FIX: Always call next() to pass the request down the pipeline.
            // Forgetting this would cause the middleware to swallow the request and
            // return an empty response.
            await _next(context);

            // Log the outgoing response status code
            _logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        }
    }

    /// <summary>
    /// Extension method to register the LoggingMiddleware in the pipeline cleanly.
    /// </summary>
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
