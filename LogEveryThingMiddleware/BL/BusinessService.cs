namespace LogEveryThingMiddleware.BL
{
    public class BusinessService : IBusinessService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BusinessService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> DoStuff()
        {
            var client = _httpClientFactory.CreateClient(ConstantNames.InternalHttpClient);

            HttpResponseMessage response = await client.GetAsync("http://demo.com/cool-stuff");

            return response.IsSuccessStatusCode;
        }
    }
}
