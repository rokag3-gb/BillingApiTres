///--------------2024/12/30 BillDetail 테이블 사용하지 않음으로 결정.
///--------------BillDetail 테이블의 KeyId와 VenderCode는 BillItem으로 이동.
///--------------하나의 bill item에서 여러 벤더사 데이터 또는 여러 마스터 데이터를 사용하지 않는다

//using AutoMapper;
//using Azure;
//using Billing.Data.Interfaces;
//using BillingApiTres.Models.Dto;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Immutable;

//namespace BillingApiTres.Controllers.Bills
//{
//    [Route("[controller]")]
//    [Authorize]
//    public class BillDetailController(IBillDetailRepository billItemRepository,
//                                      IMapper mapper,
//                                      IConfiguration config) : ControllerBase
//    {
//        [HttpGet("/bills/{billId}/billDetails")]
//        public ActionResult<PaginationResponse<BillDetailResponse>> GetBillDetails(long billId,
//                                                                     [FromQuery] BillDetailRequest request)
//        {
//            var billDetails = billItemRepository.GetList(billId, request.Offset, request.Limit);

//            if (billDetails.Any() == false)
//                return Ok(new PaginationResponse<BillDetailResponse>());

//            var accountId = new[] 
//            { 
//                billDetails.FirstOrDefault()?.BillItem?.Bill?.SellerAccountId, 
//                billDetails.FirstOrDefault()?.BillItem?.Bill?.BuyerAccountId 
//            };
//            var availableAccountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;

//            if (availableAccountIds?.Any(a => accountId.Contains(a)) == false)
//                return Forbid();

//            var ncpDetails = billDetails.SelectMany(bd => bd.NcpDetails);

//            var allDetails = ncpDetails;

//            ///벤더가 추가된다면 이렇게 할 것.
//            //var awsDetails = billDetails.SelectMany(bd => bd.AwsDetails)
//            //    .Select(n => new BillDetailResponse
//            //    {
//            //        BillId = n.BillDetail.BillItem.BillId,
//            //        BillItemId = n.BillDetail.BillItem.BillItemId,
//            //        BillDetailId = n.BillDetail.BillDetailId,
//            //        KeyId = n.KeyId,
//            //        DetailLineId = n.DetailLineId,
//            //        DemandType = n.DemandTypeCodeName,
//            //        DemandTypeDetail = n.DemandTypeDetailCodeName,
//            //        UnitUsageQuantity = n.UnitUsageQuantity
//            //    });
//            //var allDetails = ncpDetails.Concat(awsDetails);

//            var response = allDetails.Select(d => mapper.Map<BillDetailResponse>(d, opt =>
//            {
//                opt.AfterMap((o, r) =>
//                {
//                    r.BillId = d.BillDetail.BillItem.BillId;
//                    r.BillItemId = d.BillDetail.BillItemId;
//                    r.BillDetailId = d.BillDetail.BillDetailId;
//                });
//            }));
//            return new PaginationResponse<BillDetailResponse>(response,
//                                                              allDetails.Count(),
//                                                              request.Offset,
//                                                              request.Limit);
//        }
//    }
//}
