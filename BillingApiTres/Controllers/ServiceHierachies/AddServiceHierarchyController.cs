using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using BillingApiTres.Controllers.Tenants;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class AddServiceHierarchyController(
        IServiceHierarchyRepository serviceHierarchyRepository,
        AcmeGwClient gwClient,
        IMapper mapper,
        ILogger<AddServiceHierarchyController> logger) : ControllerBase
    {
        [HttpPost("/service-organizations")]
        public async Task<ActionResult<ServiceHierarchyResponse>> Add(
            [FromBody]ServiceHierarchyAddRequest addRequest)
        {
            if (addRequest == null)
                return BadRequest(new ArgumentNullException(nameof(addRequest)));

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var entity = mapper.Map<ServiceHierarchyAddRequest, ServiceHierarchy>(addRequest);
            entity.SavedAt = DateTime.UtcNow;
            entity.SaverId = token?.Subject;

            var accounts = await gwClient.Get<List<SalesAccount>>("sales/account?limit=99999", token?.RawData!);

            var addedEntity = await serviceHierarchyRepository.Add(entity);
            var returnDto = mapper.Map<ServiceHierarchyResponse>(addedEntity, options =>
            {
                options.Items["accounts"] = accounts;
            });

            //return CreatedAtAction(nameof(GetServiceHierachyController.Get), new { serialNo = returnDto.SerialNo}, returnDto);
            return Ok(returnDto);
        }
    }
}
