using FakeItEasy;
using LogEveryThingMiddleware.BL;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace LogEveryThingMiddleware.Tests
{
    [TestClass]
    public class LogEveryThingMiddlewareTests
    {
        [TestMethod]
        public async Task LogEveryThingMiddleware_ShouldNotLog()
        {
            var logService = A.Fake<ILogService>();

            var logMiddleware = new LogsMiddleware(
                innerHttpContext => Task.CompletedTask,
                logService);

            var context = new DefaultHttpContext
            {
                Response = { Body = new MemoryStream() }
            };

            //Call the middleware
            await logMiddleware.InvokeAsync(context);

            A.CallTo(() => logService.Log(A<string>._)).MustNotHaveHappened();
        }


        [TestMethod]
        public async Task LogEveryThingMiddleware_ShouldLog_FirstCall()
        {
            var logService = A.Fake<ILogService>();

            var logMiddleware = new LogsMiddleware(
                innerHttpContext => Task.CompletedTask,
                logService);

            var context = new DefaultHttpContext
            {
                Response = { Body = new MemoryStream() }
            };

            //add stuff to the request header
            context.Request.Headers.Append("x-master-log-should-log", "true");

            //add data to request body
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"userName\" :\"John\"}"));

            //add stuff to the query string
            context.Request.QueryString = context.Request.QueryString.Add("filter", "1");

            //Call the middleware
            await logMiddleware.InvokeAsync(context);
            await context.Response.StartAsync();

            context.Response.Headers.TryGetValue("x-master-log-trace-id", out var traceIdValue);
            context.Response.Headers.TryGetValue("x-master-log-level", out var levelValue);

            Assert.AreEqual(levelValue.ToString(), "1");

            A.CallTo(() => logService.Log(A<string>.That.Matches(x => ValidateString(x, 1, "John", "1", traceIdValue))))
                .MustHaveHappened();

        }

        [TestMethod]
        public async Task LogEveryThingMiddleware_ShouldLog_SecondCall()
        {
            var logService = A.Fake<ILogService>();

            var logMiddleware = new LogsMiddleware(
                innerHttpContext => Task.CompletedTask,
                logService);

            var context = new DefaultHttpContext
            {
                Response = { Body = new MemoryStream() }
            };

            //add stuff to the request header
            context.Request.Headers.Append("x-master-log-should-log", "true");
            context.Request.Headers.Append("x-master-log-level", "3");
            var traceId = Guid.NewGuid().ToString("N");
            context.Request.Headers.Append("x-master-log-trace-id", traceId);
            //add data to request body
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"userName\" :\"John\"}"));

            //add stuff to the query string
            context.Request.QueryString = context.Request.QueryString.Add("filter", "1");

            //Call the middleware
            await logMiddleware.InvokeAsync(context);
            await context.Response.StartAsync();

            context.Response.Headers.TryGetValue("x-master-log-trace-id", out var traceIdValue);
            context.Response.Headers.TryGetValue("x-master-log-level", out var levelValue);

            Assert.AreEqual(traceIdValue.ToString(), traceId);
            Assert.AreEqual(levelValue.ToString(), "1");

            A.CallTo(() => logService.Log(A<string>.That.Matches(x => ValidateString(x, 3, "John", "1", traceId))))
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
}
