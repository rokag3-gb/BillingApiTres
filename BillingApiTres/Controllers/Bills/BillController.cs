using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Extensions;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.Bills
{
    [Route("[controller]")]
    [Authorize]
    public class BillController(IBillRepository billRepository,
                                AcmeGwClient gwClient,
                                ITimeZoneConverter timeZoneConverter,
                                IMapper mapper,
                                IConfiguration config,
                                CurrencyConverter currencyConverter,
                                ILogger<BillController> logger) : ControllerBase
    {
        /// <summary>
        /// 지정한 기간 내 특정 고객사들의 청구서를 조회합니다
        /// </summary>
        [AuthorizeAccountIdFilter([nameof(request.AccountIds)])]
        [HttpGet("/bills")]
        public async Task<ActionResult<List<BillResponse>>> GetList([FromQuery] BillListRequest request)
        {
            if (request.From.CompareTo(request.To) > 0)
                return BadRequest($"검색 기간 설정 오류 : {request.From} ~ {request.To}");

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            var accountIds = request.AccountIds
                .Split(",", StringSplitOptions.TrimEntries)
                .Select(s =>
                {
                    if (long.TryParse(s, out long id))
                        return id;
                    return -1;
                })
                .Where(i => i >= 1).ToList();

            //get bills
            var bills = billRepository.GetRange(timeZoneConverter.ConvertToUtc(request.From, tz!),
                                                timeZoneConverter.ConvertToUtc(request.To, tz!),
                                                accountIds,
                                                null,
                                                request.Offset,
                                                request.limit);

            //get status codes
            var codes = new List<SaleCode>();
            var requestCodeKinds = bills.Select(b => b.StatusCode.Split("-").First()).Distinct();

            foreach (var kind in requestCodeKinds)
            {
                var ret = await gwClient.Get<List<SaleCode>>($"sales/code/{kind}/childs", token?.RawData!);
                codes.AddRange(ret);
            }

            //get currency codes
            var currencyInfos = bills.Select(b => b.CurrencyCode)
                .Distinct()
                .Select(currencyConverter.GetCurrencyInfo)
                .Where(c => c != null);

            //get userentity
            var userIds = bills
                .SelectMany(b => new[] { b.SellerManagerId, b.BuyerManagerId, b.SaverId })
                .Distinct()
                .Where(s => string.IsNullOrEmpty(s) == false);
            var users = await gwClient.Get<List<IamUserEntity>>($"iam/users?ids={string.Join(",", userIds)}", token?.RawData!);

            //get accounts
            var usedAccountIds = bills.SelectMany(b => new[] { b.SellerAccountId, b.BuyerAccountId, b.ConsumptionAccountId }).Distinct().Where(a => a != null).Cast<long>();
            var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join(",", usedAccountIds)}", token?.RawData!);

            var response = bills.Select(b =>
            {
                return mapper.Map<BillResponse>(b, opt =>
                {
                    opt.AfterMap((o, br) =>
                    {
                        var currencyInfo = currencyInfos.FirstOrDefault(c => c?.CurrencyCode == b.CurrencyCode);

                        br.SellerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.SellerAccountId)?.AccountName ?? string.Empty;
                        //br.BuyerAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountKey1 ?? string.Empty;
                        br.BuyerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountName ?? string.Empty;
                        //br.ConsumptionAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountKey1 ?? string.Empty;
                        br.ConsumptionAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountName ?? string.Empty;
                        br.StatusName = codes?.FirstOrDefault(c => c.Code == b.StatusCode)?.Name ?? string.Empty;
                        br.CurrencyName = currencyInfo?.CurrencyEnglishName ?? string.Empty;
                        br.CurrencySymbol = currencyInfo?.CurrencySymbol ?? string.Empty;
                        br.BuyerManageName = users?.FirstOrDefault(u => u.Id == b.BuyerManagerId)?.Name ?? string.Empty;
                        br.SellerManageName = users?.FirstOrDefault(u => u.Id == b.SellerManagerId)?.Name ?? string.Empty;
                        br.SaverName = users?.FirstOrDefault(u => u.Id == b.SaverId)?.Name ?? string.Empty;
                        br.BillDate = timeZoneConverter.ConvertToLocal(b.BillDate, tz);
                        if (b.ConsumptionStartDate.HasValue)
                            br.ConsumptionStartDate = timeZoneConverter.ConvertToLocal(b.ConsumptionStartDate.Value, tz);
                        if (b.ConsumptionEndDate.HasValue)
                            br.ConsumptionEndDate = timeZoneConverter.ConvertToLocal(b.ConsumptionEndDate.Value, tz);
                        br.SavedAt = timeZoneConverter.ConvertToLocal(b.SavedAt, tz);
                    });
                });
            }).ToList();

            return response;
        }

        /// <summary>
        /// 특정 청구서들의 상태를 갱신합니다
        /// </summary>
        [HttpPut("/bills/status")]
        public async Task<ActionResult> Update([FromBody] BillUpdateRequest request)
        {
            var bills = billRepository.GetRange(null, null, null, request.BillIds, null, null);

            if (bills?.Any() == false)
                return NotFound($"not found bill resource : {string.Join(",", request.BillIds)}");

            if (HttpContext.AuthenticateAccountId(bills!.Select(b => b.BuyerAccountId)) == false)
                return Forbid();

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var billStatus = await gwClient.Get<List<SaleCode>>($"sales/code/BST/childs", token?.RawData!);
            var billStatusCodes = billStatus.Select(bs => bs.Code).ToHashSet();

            if (billStatusCodes.Contains(request.StatusCode) == false)
                return BadRequest($"적절하지 않은 상태 코드 요청 입니다 : {request.StatusCode} - {string.Join(", " , billStatusCodes)} 만 유효합니다");
            
            billRepository.UpdateStatus(request.StatusCode, request.BillIds);

            return Ok();
        }
    }
}
