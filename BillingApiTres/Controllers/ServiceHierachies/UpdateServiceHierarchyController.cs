using AutoMapper;
using Azure.Core;
using Billing.Data.Interfaces;
using BillingApiTres.Controllers.Tenants;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class UpdateServiceHierarchyController(
        IServiceHierarchyRepository serviceHierachyRepository,
        IMapper mapper,
        ILogger<GetTenantController> logger) : ControllerBase
    {
        [HttpPut("/service-organizations/{serialNo}")]
        public async Task<ActionResult> UpdateServiceHierarchy(
            int serialNo,
            [FromBody]ServiceHierarchyUpdateRequest updateRequest)
        {
            var entity = await serviceHierachyRepository.Get(serialNo);

            if (entity == null)
                return NotFound();
            
            entity.IsActive = updateRequest.IsActive ?? entity.IsActive;
            entity.StartDate = updateRequest.ContractDate?.ToUniversalTime() ?? entity.StartDate;
            entity.EndDate = updateRequest.ExpireDate?.ToUniversalTime() ?? entity.EndDate;
            entity.SavedAt = DateTime.UtcNow;

            await serviceHierachyRepository.Update(entity);
            
            return Ok(serialNo);
        }
    }
}
