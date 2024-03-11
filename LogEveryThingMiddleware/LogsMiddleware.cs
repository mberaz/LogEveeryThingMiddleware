using System.Diagnostics;
using System.Text.Json.Nodes;
using LogEveryThingMiddleware.BL;
using LogEveryThingMiddleware.Trace;
using Microsoft.AspNetCore.Http.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogEveryThingMiddleware
{
    public class LogsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogService _logService;

        public LogsMiddleware(RequestDelegate next, ILogService logService)
        {
            _next = next;
            _logService = logService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("x-master-log-should-log", out var shouldLog);

            if (shouldLog == "true")
            {
                var wasTraceIdFound = httpContext.Request.Headers.TryGetValue("x-master-log-trace-id", out var traceIdValue);
                var wasLevelFound = httpContext.Request.Headers.TryGetValue("x-master-log-level", out var levelValue);

                var data = new TraceData
                {
                    TraceId = wasTraceIdFound ? traceIdValue.ToString() : Guid.NewGuid().ToString("N"),
                    Level = wasLevelFound ? int.Parse(levelValue.ToString()) : 1
                };

                httpContext.Items.Add("data", data);
                TraceStorage<TraceData>.Store(data);

                await LogRequest(httpContext.Request, "DemoService", data);

                //Adds a delegate to be invoked just before response headers will be sent to the client.
            }

            httpContext.Response.OnStarting(state =>
            {
                httpContext.Items.TryGetValue("data", out var data);
                if (data != null)
                {
                    httpContext.Response.Headers.TryAdd("x-master-log-trace-id", ((TraceData)data).TraceId);
                }
                return Task.CompletedTask;
            }, httpContext);

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
            await _logService.Log(logString);
        }
    }
}
