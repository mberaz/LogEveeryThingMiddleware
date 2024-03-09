namespace LogEveryThingMiddleware
{
    public class LogsMiddleware
    {
        private readonly RequestDelegate _next;

        public LogsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("x-should-log", out var shouldLog);

            //https://stackoverflow.com/questions/50747749/how-to-use-httpclienthandler-with-httpclientfactory-in-net-core

            // Call the next delegate/middleware in the pipeline.
            await _next(httpContext);

        }
    }
}
