using System.Text.Json.Nodes;
using LogEveryThingMiddleware.Trace;
using Microsoft.AspNetCore.Http.Extensions;

namespace LogEveryThingMiddleware.BL
{
    public class BusinessService : IBusinessService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BusinessService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task DoStuff()
        {
            var client = _httpClientFactory.CreateClient(ConstantNames.InternalHttpClient);

            HttpResponseMessage response = await client.GetAsync("http://demo.com/cool-stuff");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

     
    }
}
