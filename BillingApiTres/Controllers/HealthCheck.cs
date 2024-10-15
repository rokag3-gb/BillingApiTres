using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers
{
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(ILogger<HealthCheckController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            return await Task.FromResult("OK");
        }
    }
}
