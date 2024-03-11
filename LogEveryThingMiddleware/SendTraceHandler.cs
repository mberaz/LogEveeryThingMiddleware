using LogEveryThingMiddleware.BL;
using LogEveryThingMiddleware.Trace;

namespace LogEveryThingMiddleware;

public class SendTraceHandler : DelegatingHandler
{
    private readonly ILogService _logService;

    public SendTraceHandler(ILogService logService)
    {
        _logService = logService;
    }

    public SendTraceHandler(ILogService logService, HttpClientHandler innerHandler)
    {
        _logService = logService;
        InnerHandler = innerHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var traceData = TraceStorage<TraceData>.Retrieve();
        if (traceData != null)
        {
            AddTraceHeadersToRequest(request, traceData);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (traceData != null)
        {
            await Log(CreateRequestString(request, traceData), response);
        }

        return response;
    }

    private void AddTraceHeadersToRequest(HttpRequestMessage request, TraceData data)
    {
        request.Headers.Add("x-master-log-should-log", "true");
        request.Headers.Add("x-master-log-trace-id", data.TraceId);
        request.Headers.Add("x-master-log-level", data.Level.ToString());
    }

    private string CreateRequestString(HttpRequestMessage request, TraceData traceData)
    {
        var logString = $"[{traceData.Level}] [{traceData.TraceId}] Sending the request :{request.Method};" +
                        $"{request.RequestUri}";

        logString += ";headers:";
        foreach (var requestHeader in request.Headers)
        {
            logString += $"{requestHeader.Key}-{requestHeader.Value}";
        }

        if (request.Content != null)
        {
            StreamReader reader = new StreamReader(request.Content?.ReadAsStream());
            string body = reader.ReadToEnd();
            logString += $";body:{body}";
        }


        return logString;
    }


    private async Task Log(string requestString, HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();

        var logString = $"{requestString}: Response:[{response.StatusCode}] {responseBody}";
        //log the string
        await _logService.Log(logString);
    }
}