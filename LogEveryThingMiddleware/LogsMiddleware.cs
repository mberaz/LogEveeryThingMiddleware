using System.Text.Json.Nodes;
using LogEveryThingMiddleware.Trace;
using Microsoft.AspNetCore.Http.Extensions;

namespace LogEveryThingMiddleware
{
    public class LogsMiddleware
    {
        private readonly RequestDelegate _next;

        public LogsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext )
        {
            httpContext.Request.Headers.TryGetValue("x-should-log", out var shouldLog);

            if (shouldLog == "true")
            {
                var wasTraceIdFound = httpContext.Request.Headers.TryGetValue("x-should-trace-id", out var traceIdValue);
                var wasLevelFound = httpContext.Request.Headers.TryGetValue("x-should-level", out var levelValue);

                var data = new TraceData
                {
                    TraceId = wasTraceIdFound ? traceIdValue.ToString() : Guid.NewGuid().ToString("N"),
                    Level = wasLevelFound ? int.Parse(levelValue.ToString()) : 1
                };
                TraceStorage<TraceData>.Store(data);

                await LogRequest(httpContext.Request, "DemoService", data);
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(httpContext);

        }

        private async Task LogRequest(HttpRequest request, string currentServiceName, TraceData traceData)
        {
            var logString = $"[{traceData.Level}] [{traceData.TraceId}] {currentServiceName} Got the request :" +
                            $"{request.Method};{request.GetDisplayUrl()}";

            foreach (var queryValue in request.Query)
            {
                logString += $"{queryValue.Key}-{queryValue.Value}";
            }

            logString += ";headers:";
            foreach (var requestHeader in request.Headers)
            {
                logString += $"{requestHeader.Key}-{requestHeader.Value}";
            }

            if (request.Body.CanSeek)
            {
                var bodyObject = await JsonNode.ParseAsync(request.Body);
                logString += $";body:{bodyObject?.ToJsonString()}";
            }


            //log the string

        }
    }
}
