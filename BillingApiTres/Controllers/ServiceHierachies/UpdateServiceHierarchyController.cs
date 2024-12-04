using AutoMapper;
using Azure;
using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class UpdateServiceHierarchyController(
        IServiceHierarchyRepository serviceHierachyRepository,
        IMapper mapper,
        IConfiguration config,
        ITimeZoneConverter timeZoneConverter,
        ILogger<UpdateServiceHierarchyController> logger) : ControllerBase
    {
        [HttpPut("/service-organizations/{serialNo}")]
        public async Task<ActionResult<long>> UpdateServiceHierarchy(
            long serialNo,
            [FromBody] ServiceHierarchyUpdateRequest updateRequest)
        {
            var entity = await serviceHierachyRepository.Get(serialNo);

            if (entity == null)
                return Conflict($"대상이 존재하지 않습니다 : {updateRequest}");

            var accountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;
            if (accountIds?.Contains(entity.AccountId) == false)
                return Forbid();

            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            entity.IsActive = updateRequest.IsActive ?? entity.IsActive;
            entity.SavedAt = DateTime.UtcNow;
            if (updateRequest.ContractDate.HasValue)
                entity.StartDate = timeZoneConverter.ConvertToUtc(updateRequest.ContractDate.Value, tz);
            if (updateRequest.ExpireDate.HasValue)
                entity.EndDate = timeZoneConverter.ConvertToUtc(updateRequest.ExpireDate.Value, tz);

            foreach (var updateConfig in updateRequest.Configs?.OrderBy(c => c.ConfigId) ?? Enumerable.Empty<ServiceHierarchyConfigUpdateRequest>())
            {
                if (updateConfig.ConfigId is null || updateConfig.ConfigId == 0)
                {
                    entity.ServiceHierarchyConfigs.Add(
                        new Billing.Data.Models.ServiceHierarchyConfig
                        {
                            Sno = entity.Sno,
                            ConfigCode = updateConfig.Code,
                            ConfigValue = updateConfig.Value
                        });

                    continue;
                }

                var config = entity.ServiceHierarchyConfigs.FirstOrDefault(c => c.ConfigId == updateConfig.ConfigId);

                if (config is null)
                {
                    var msg = $"존재하지 않는 ServiceHierarchyConfig 레코드를 수정하려고 했습니다. config id : {updateConfig.ConfigId}";
                    logger.LogError(msg);
#if DEBUG
                    return BadRequest(msg);
#else
                    return BadRequest();
#endif
                }

                config.ConfigValue = updateConfig.Value;
            }

            var codeGroup = entity.ServiceHierarchyConfigs.GroupBy(c => c.ConfigCode);
            if (codeGroup.Any(g => g.Count() > 1))
            {
                string msg = $"config code를 중복하여 등록할 수 없습니다. code : {string.Join(", ", codeGroup.Where(g => g.Count() > 1).SelectMany(g => g.Select(c => c.ConfigCode)))}";
                    
                logger.LogError(msg);
                return BadRequest(msg);
            }

            await serviceHierachyRepository.Update(entity);

            return Ok(serialNo);
        }
    }
}
