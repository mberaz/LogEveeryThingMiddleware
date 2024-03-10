using LogEveryThingMiddleware.BL;
using Microsoft.AspNetCore.Mvc;

namespace LogEveryThingMiddleware.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public DemoController( IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet(Name = "demo")]
        public async Task<ActionResult> Get()
        {
            await _businessService.DoStuff();
            return Ok();
        }
    }
}
