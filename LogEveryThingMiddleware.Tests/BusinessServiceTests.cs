using FakeItEasy;
using LogEveryThingMiddleware.BL;
using System.Net;

namespace LogEveryThingMiddleware.Tests
{
    [TestClass]
    public class BusinessServiceTests
    {
        [TestMethod]
        public async Task DoBusinessStuff_ValidResponse()
        {
            var fakeIHttpClientFactory = A.Fake<IHttpClientFactory>();
            var mockHttpClient = new HttpClient(new MockHandler("http://demo.com/cool-stuff",
                new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            }));

            A.CallTo(() => fakeIHttpClientFactory.
                CreateClient(ConstantNames.InternalHttpClient)).Returns(mockHttpClient);

            var service = new BusinessService(fakeIHttpClientFactory);

            var response = await service.DoBusinessStuff();
            Assert.IsTrue(response);
        }

        [TestMethod]
        public async Task DoBusinessStuff_NoValidResponse()
        {
            var fakeIHttpClientFactory = A.Fake<IHttpClientFactory>();
            var mockHttpClient = new HttpClient(new MockHandler("http://demo.com/cool-stuff", 
                new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            }));
            A.CallTo(() => fakeIHttpClientFactory
                .CreateClient(ConstantNames.InternalHttpClient)).Returns(mockHttpClient);

            var service = new BusinessService(fakeIHttpClientFactory);

            var response = await service.DoBusinessStuff();
            Assert.IsFalse(response);
        }
    }

    class MockHandler : DelegatingHandler
    {
        private readonly string _expectedUrl;
        private readonly HttpResponseMessage _expectedResponse;

        public MockHandler(string expectedUrl, HttpResponseMessage expectedResponse) :
                                                        base(new HttpClientHandler())
        {
            _expectedUrl = expectedUrl;
            _expectedResponse = expectedResponse;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri.ToString().Contains(_expectedUrl))
            {
                return _expectedResponse;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
