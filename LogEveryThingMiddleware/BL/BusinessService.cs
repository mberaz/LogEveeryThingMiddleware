namespace LogEveryThingMiddleware.BL
{
    public class BusinessService(IHttpClientFactory httpClientFactory) : IBusinessService
    {
        public async Task<bool> DoBusinessStuff()
        {
            var client = httpClientFactory.CreateClient(ConstantNames.InternalHttpClient);

            var response = await client.GetAsync("http://demo.com/cool-stuff");

            return response.IsSuccessStatusCode;
        }
    }
}
