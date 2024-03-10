namespace LogEveryThingMiddleware.Trace
{
    //https://vainolo.com/2022/02/23/storing-context-data-in-c-using-asynclocal/
    public static class TraceStorage<T> where T : new()
    {
        private static readonly AsyncLocal<T> _asyncLocal = new AsyncLocal<T>();
        public static T Store(T val)
        {
            _asyncLocal.Value = val;
            return _asyncLocal.Value;
        }

        public static T Retrieve()
        {
            return _asyncLocal.Value;
        }
    }
}

