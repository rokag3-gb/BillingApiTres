using Billing.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class DeleteServiceHierarchyController(
        IServiceHierarchyRepository serviceHierarchyRepository,
        ILogger<DeleteServiceHierarchyController> logger) : ControllerBase
    {

        [HttpDelete("/service-organizations/{serialNo}")]
        public async Task<ActionResult> Delete(long serialNo)
        {
            var entity = await serviceHierarchyRepository.Get(serialNo);

            if (entity == null)
                return NotFound(new { serialNo = serialNo });

            await serviceHierarchyRepository.Delete(entity);

            return NoContent();
        }
    }
}
