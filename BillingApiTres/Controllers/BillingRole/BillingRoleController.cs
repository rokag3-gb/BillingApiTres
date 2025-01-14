using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.BillingRole
{
    [Route("[controller]")]
    [Authorize]
    public class BillingRoleController(
        IBillRoleRepository billRoleRepository,
        IMapper mapper) : ControllerBase
    {
        [HttpGet("/billingRoles")]
        public ActionResult<List<BillRoleResponse>> GetAll()
        {
            var roles = billRoleRepository.GetAll();
            return mapper.Map<List<BillRoleResponse>>(roles);
        }
    }
}
