using Billing.Data.Interfaces;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class DeleteServiceHierarchyController(
        IServiceHierarchyRepository serviceHierarchyRepository,
        IConfiguration config,
        ILogger<DeleteServiceHierarchyController> logger) : ControllerBase
    {
        [HttpDelete("/service-organizations/{serialNo}")]
        public async Task<ActionResult> Delete(long serialNo)
        {
            var entity = await serviceHierarchyRepository.Get(serialNo);
            if (entity == null)
                return NoContent();

            var accountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;
            if (accountIds?.Contains(entity.AccountId) == false)
                return Forbid();

            await serviceHierarchyRepository.Delete(entity);

            return NoContent();
        }
    }
}
