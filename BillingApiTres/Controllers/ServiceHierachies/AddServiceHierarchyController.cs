using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models.Iam;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class AddServiceHierarchyController(
        IServiceHierarchyRepository serviceHierarchyRepository,
        IAccountKeyRepository accountKeyRepository,
        AcmeGwClient gwClient,
        IMapper mapper,
        IConfiguration config,
        ITimeZoneConverter timeZoneConverter,
        ILogger<AddServiceHierarchyController> logger) : ControllerBase
    {
        [AuthorizeAccountIdFilter([nameof(ServiceHierarchyAddRequest.ContractorId)])]
        [HttpPost("/service-organizations")]
        public async Task<ActionResult<ServiceHierarchyResponse>> Add(
            [FromBody]ServiceHierarchyAddRequest addRequest)
        {
            if (addRequest == null)
                return BadRequest(new ArgumentNullException(nameof(addRequest)));

            var isInvalid = serviceHierarchyRepository.CheckInvalidation(addRequest.ContractorId, addRequest.ContracteeId);
            if (isInvalid)
                return BadRequest("유효하지 않은 account 설정 입니다");

            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var entity = mapper.Map<ServiceHierarchyAddRequest, ServiceHierarchy>(addRequest);
            entity.SavedAt = DateTime.UtcNow;
            entity.SaverId = token?.Subject;
            entity.StartDate = timeZoneConverter.ConvertToUtc(addRequest.ContractDate, tz);
            entity.EndDate = timeZoneConverter.ConvertToUtc(addRequest.ExpireDate, tz);

            var accounts = await gwClient
                .Get<List<SalesAccount>>($"sales/account?limit=99999&accountIds={entity.ParentAccId},{entity.AccountId}", token?.RawData!);

            var accountLinks = await gwClient.Get<List<AccountLink>>($"sales/accountLink?limit=999999&offset=0&accountIdCsv={entity.AccountId}", token?.RawData!);
            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=999999&offset=0&accountIdCsv={entity.AccountId}", token?.RawData!);

            var addedEntity = await serviceHierarchyRepository.Add(entity);
            var returnDto = mapper.Map<ServiceHierarchyResponse>(addedEntity, options =>
            {
                options.Items["accounts"] = accounts;
                options.Items["accountLink"] = accountLinks;
                options.Items["accountUser"] = accountUsers;
                options.Items["timezone"] = tz;
            });

            //return CreatedAtAction(nameof(GetServiceHierachyController.Get), new { serialNo = returnDto.SerialNo}, returnDto);
            return Ok(returnDto);
        }
    }
}
