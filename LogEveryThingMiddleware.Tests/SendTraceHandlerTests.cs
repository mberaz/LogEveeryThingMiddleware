using FakeItEasy;
using LogEveryThingMiddleware.BL;
using LogEveryThingMiddleware.Trace;
using System.Net.Http.Json;

namespace LogEveryThingMiddleware.Tests
{
    [TestClass]
    public class SendTraceHandlerTests
    {
        [TestMethod]
        public async Task SendRequest_DoNotLog()
        {
            var logService = A.Fake<ILogService>();
            var handler = new SendTraceHandler(logService, new HttpClientHandler());
            var client = new HttpClient(handler);
            var response = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://demo.com/cool-stuff?filter=1")
            });
            A.CallTo(() => logService.Log(A<string>._)).MustNotHaveHappened();
        }

        [TestMethod]
        public async Task SendRequest_ShouldLog()
        {
            var traceId = Guid.NewGuid().ToString("N");
            var logService = A.Fake<ILogService>();
            var handler = new SendTraceHandler(logService, 
                new MockHeadersHandler(new Dictionary<string, string>
            {
                {"x-master-log-should-log","true"},
                {"x-master-log-trace-id",traceId},
                {"x-master-log-level","1"}
            }));

            var client = new HttpClient(handler);


            var data = new TraceData
            {
                TraceId = traceId,
                Level = 1
            };
            TraceStorage<TraceData>.Store(data);

            var response = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://demo.com/cool-stuff?filter=1"),
                Content = JsonContent.Create(new { firstName = "John" })
            });

            A.CallTo(() => logService.Log(A<string>.That.Matches(x => ValidateString(x, 1, "John", "1", traceId))))
                .MustHaveHappened();
        }


        private bool ValidateString(string log, int expectedLevel,
            string bodyValue, string qsValue, string? expectedTraceId = null)
        {
            Assert.IsTrue(log.Contains($"[{expectedLevel}]"));
            Assert.IsTrue(log.Contains($"{bodyValue}"));
            Assert.IsTrue(log.Contains($"{qsValue}"));

            if (expectedTraceId != null)
            {
                Assert.IsTrue(log.Contains($"[{expectedTraceId}]"));
            }

            return true;
        }
    }

    class MockHeadersHandler : HttpClientHandler
    {
        private readonly Dictionary<string, string> _excpected;

        public MockHeadersHandler(Dictionary<string, string> excpected)
        {
            _excpected = excpected;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            foreach (var item in _excpected)
            {
                var headerValue = request.Headers.GetValues(item.Key);
                Assert.AreEqual(headerValue.First(), item.Value);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
