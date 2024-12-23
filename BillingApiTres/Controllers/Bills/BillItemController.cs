using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace BillingApiTres.Controllers.Bills
{
    [Route("[controller]")]
    [Authorize]
    public class BillItemController(IBillItemRepository billItemRepository,
                                    IMapper mapper,
                                    IConfiguration config) : ControllerBase
    {
        [HttpGet("/bills/{billId}/billItems")]
        public ActionResult<PaginationResponse<BillItemResponse>> GetList(
            long billId,
            [FromQuery] BillItemRequest request)
        {
            var items = billItemRepository.GetList(billId, request.Offset, request.Limit);

            if (items.Any() == false)
                return Ok(new());

            var accountId = new[] 
            { 
                items.FirstOrDefault()?.Bill?.SellerAccountId, 
                items.FirstOrDefault()?.Bill?.BuyerAccountId 
            };
            var availableAccountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;

            if (availableAccountIds?.Any(a => accountId.Contains(a)) == false)
                return Forbid();

            var response = items.Select(i => mapper.Map<BillItemResponse>(i)).ToList();

            return new PaginationResponse<BillItemResponse>(response,
                                                            items.Count,
                                                            request.Offset,
                                                            request.Limit);
        }
    }
}
