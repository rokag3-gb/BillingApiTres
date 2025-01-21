using AutoMapper;
using Billing.Data.Interfaces;
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

        [HttpGet("/bills/{billId}/billDetails")]
        public ActionResult<PaginationResponse<BillDetailResponse>> GetBillDetails(long billId,
                                                                     [FromQuery] BillDetailRequest request)
        {
            var billDetails = billItemRepository.GetListVenderDetail(billId, request.Offset, request.Limit);

            if (billDetails.Any() == false)
                return Ok(new PaginationResponse<BillDetailResponse>());

            var accountId = new[]
            {
                billDetails.FirstOrDefault()?.Bill?.SellerAccountId,
                billDetails.FirstOrDefault()?.Bill?.BuyerAccountId
            };
            var availableAccountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;

            if (availableAccountIds?.Any(a => accountId.Contains(a)) == false)
                return Forbid();

            var ncpDetails = billDetails.Select(bd => new { bd.BillId, bd.BillItemId, bd.NcpDetails });

            var allDetails = ncpDetails;

            ///벤더가 추가된다면 이렇게 할 것.
            //var awsDetails = billDetails.SelectMany(bd => bd.AwsDetails)
            //    .Select(n => new BillDetailResponse
            //    {
            //        BillId = n.BillDetail.BillItem.BillId,
            //        BillItemId = n.BillDetail.BillItem.BillItemId,
            //        BillDetailId = n.BillDetail.BillDetailId,
            //        KeyId = n.KeyId,
            //        DetailLineId = n.DetailLineId,
            //        DemandType = n.DemandTypeCodeName,
            //        DemandTypeDetail = n.DemandTypeDetailCodeName,
            //        UnitUsageQuantity = n.UnitUsageQuantity
            //    });
            //var allDetails = ncpDetails.Concat(awsDetails);

            var response = allDetails.SelectMany(
                r => r.NcpDetails.Select(
                    d => mapper.Map<BillDetailResponse>(d, opt =>
                    {
                        opt.AfterMap((o, bd) =>
                        {
                            bd.BillId = r.BillId;
                            bd.BillItemId = r.BillItemId;
                        });
                    })));

            return new PaginationResponse<BillDetailResponse>(response,
                                                              response.Count(),
                                                              request.Offset,
                                                              request.Limit);
        }
    }
}
