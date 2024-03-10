﻿using LogEveryThingMiddleware.BL;
using LogEveryThingMiddleware.Trace;

namespace LogEveryThingMiddleware;

public class SendTraceHandler : DelegatingHandler
{
    private readonly ILogService _logService;

    public SendTraceHandler(ILogService logService) : base(new HttpClientHandler())
    {
        _logService = logService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        var traceData = TraceStorage<TraceData>.Retrieve();

        if (traceData != null)
        {
            await Log(CreateRequestString(request, traceData), response);
        }

        return response;
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

        var reader = new StreamReader(request.Content?.ReadAsStream());
        logString += $";body:{reader.ReadToEnd()}";

        return logString;
    }


    private async Task Log(string requestString, HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var logString = $"{requestString}: Response:[{response.StatusCode}] {responseBody}";

        //log the string
        await _logService.Log(logString);
    }
}