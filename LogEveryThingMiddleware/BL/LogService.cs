namespace LogEveryThingMiddleware.BL
{
    public class LogService:ILogService
    {
        public Task Log(string logString)
        {
           return Task.CompletedTask;
        }
    }
}
