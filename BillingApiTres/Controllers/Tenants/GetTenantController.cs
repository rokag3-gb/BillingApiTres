using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.Tenants
{
    [Route("[controller]")]
    [Authorize]
    public class GetTenantController(
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<GetTenantController> logger) : ControllerBase
    {
        [HttpGet("/tenants")]
        public async IAsyncEnumerable<TenantResponse> GetList(int offset = 0, int limit = 50)
        {
            var tenants = tenantRepository.GetListAsync(offset, limit);

            await foreach (var tenant in tenants)
            {
                yield return mapper.Map<TenantResponse>(tenant);
            }
        }
    }
}
