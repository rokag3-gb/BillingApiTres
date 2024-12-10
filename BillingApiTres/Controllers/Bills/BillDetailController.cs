using AutoMapper;
using Azure;
using Billing.Data.Interfaces;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace BillingApiTres.Controllers.Bills
{
    [Route("[controller]")]
    [Authorize]
    public class BillDetailController(IBillDetailRepository billItemRepository,
                                      IConfiguration config) : ControllerBase
    {
        [HttpGet("/bills/{billId}/billDetails")]
        public ActionResult<PaginationResponse<BillDetailResponse>> GetBillDetails(long billId,
                                                                     [FromQuery] BillDetailRequest request)
        {
            var billDetails = billItemRepository.GetList(billId, request.Offset, request.Limit);

            if (billDetails.Any() == false)
                return NotFound(new { BillId = billId });

            var accountId = billDetails.FirstOrDefault()?.BillItem?.Bill?.SellerAccountId;
            var availableAccountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;

            if (availableAccountIds?.Contains(accountId ?? -1) == false)
                return Forbid();

            var ncpDetails = billDetails.SelectMany(bd => bd.NcpDetails)
                .Select(n => new BillDetailResponse
                {
                    BillId = n.BillDetail.BillItem.BillId,
                    BillItemId = n.BillDetail.BillItem.BillItemId,
                    BillDetailId = n.BillDetail.BillDetailId,
                    KeyId = n.KeyId,
                    DetailLineId = n.DetailLineId,
                    DemandType = n.DemandTypeCodeName,
                    DemandTypeDetail = n.DemandTypeDetailCodeName,
                    UnitUsageQuantity = n.UnitUsageQuantity
                });

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

            return new PaginationResponse<BillDetailResponse>(allDetails,
                                                              allDetails.Count(),
                                                              request.Offset,
                                                              request.Limit);
        }
    }
}
