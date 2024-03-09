namespace LogEveryThingMiddleware;

class SendTraceHandler : DelegatingHandler
{
    public SendTraceHandler()
    {
    }
     
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        //https://stackoverflow.com/questions/50747749/how-to-use-httpclienthandler-with-httpclientfactory-in-net-core


        // Add custom functionality here, before or after base.SendAsync()
        WriteLog("CustomHandlerA() - before inner handler");
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        WriteLog("CustomHandlerA() - after inner handler; response.StatusCode=" + 
                 response.StatusCode);

        return response;
    }

    public void WriteLog(string Msg)
    {
        StreamWriter sw = new StreamWriter("C:\\WebAPI21ClientDemo\\LogFile.txt", true);
        sw.WriteLine(DateTime.Now.ToString() + " " + Msg);
        sw.Close();
    }
}