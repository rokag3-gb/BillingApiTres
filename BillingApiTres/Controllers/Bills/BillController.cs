using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.Bills
{
    [Route("[controller]")]
    [Authorize]
    public class BillController(IBillRepository billRepository,
                                IAccountKeyRepository accountKeyRepository,
                                AcmeGwClient gwClient,
                                IMapper mapper,
                                CurrencyConverter currencyConverter,
                                ILogger<BillController> logger) : ControllerBase
    {
        [HttpGet("/bills")]
        public async Task<ActionResult<List<BillResponse>>> GetList([FromQuery] BillListRequest request)
        {
            if (request.From.CompareTo(request.To) > 0)
                return BadRequest($"검색 기간 설정 오류 : {request.From} ~ {request.To}");

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            //get account id
            var accountKeys = await accountKeyRepository.GetIdList(request.AccountIds);
            var accountIds = accountKeys.Select(a => a.AccountId).ToList();
            //var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join(",", accountIds)}", token?.RawData!);

            //get bills
            var bills = billRepository.GetRange(request.From, request.To, accountIds, request.Offset, request.limit);

            //get status codes
            var codes = new List<SaleCode>();
            var requestCodes = bills.Select(b => b.StatusCode).Distinct();

            foreach (var code in requestCodes)
            {
                var kind = code.Split("-").First();
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
            var usedAccountKeys = await accountKeyRepository.GetKeyList(usedAccountIds.ToList());

            var response = bills.Select(b =>
            {
                return mapper.Map<BillResponse>(b, opt =>
                {
                    opt.AfterMap((o, br) =>
                    {
                        var currencyInfo = currencyInfos.FirstOrDefault(c => c.CurrencyCode == b.CurrencyCode);

                        br.SellerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.SellerAccountId)?.AccountName ?? string.Empty;
                        br.BuyerAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountKey1 ?? string.Empty;
                        br.BuyerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountName ?? string.Empty;
                        br.ConsumptionAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountKey1 ?? string.Empty;
                        br.ConsumptionAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountName ?? string.Empty;
                        br.StatusName = codes?.FirstOrDefault(c => c.Code == b.StatusCode)?.Name ?? string.Empty;
                        br.CurrencyName = currencyInfo?.CurrencyEnglishName ?? string.Empty;
                        br.CurrencySymbol = currencyInfo?.CurrencySymbol ?? string.Empty;
                        br.BuyerManageName = users?.FirstOrDefault(u => u.Id == b.BuyerManagerId)?.Name ?? string.Empty;
                        br.SellerManageName = users?.FirstOrDefault(u => u.Id == b.SellerManagerId)?.Name ?? string.Empty;
                        br.SaverName = users?.FirstOrDefault(u => u.Id == b.SaverId)?.Name ?? string.Empty;
                    });
                });
            }).ToList();

            return response;
        }
    }
}
