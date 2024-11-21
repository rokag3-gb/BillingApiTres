using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using Billing.EF.Repositories;
using BillingApiTres.Controllers.Tenants;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class AddServiceHierarchyController(
        IServiceHierarchyRepository serviceHierarchyRepository,
        IAccountKeyRepository accountKeyRepository,
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

            var accountKeys = await accountKeyRepository
                .GetIdList(new List<string>() { addRequest.ContractorKey, addRequest.ContracteeKey });

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var entity = mapper.Map<ServiceHierarchyAddRequest, ServiceHierarchy>(addRequest);
            entity.ParentAccId = accountKeys.Where(ak => ak.AccountKey1 == addRequest.ContractorKey).First().AccountId;
            entity.AccountId = accountKeys.Where(ak => ak.AccountKey1 == addRequest.ContracteeKey).First().AccountId;
            entity.SavedAt = DateTime.UtcNow;
            entity.SaverId = token?.Subject;

            var accounts = await gwClient
                .Get<List<SalesAccount>>($"sales/account?limit=99999&accountIds={entity.ParentAccId},{entity.AccountId}", token?.RawData!);

            var addedEntity = await serviceHierarchyRepository.Add(entity);
            var returnDto = mapper.Map<ServiceHierarchyResponse>(addedEntity, options =>
            {
                options.Items["accounts"] = accounts;
                options.Items["accountKeys"] = accountKeys;
            });

            //return CreatedAtAction(nameof(GetServiceHierachyController.Get), new { serialNo = returnDto.SerialNo}, returnDto);
            return Ok(returnDto);
        }
    }
}
