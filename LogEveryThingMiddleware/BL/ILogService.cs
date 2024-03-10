namespace LogEveryThingMiddleware.BL
{
    public interface ILogService
    {
        Task Log(string logString);
    }
}
