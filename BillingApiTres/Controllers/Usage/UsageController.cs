using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.Usage
{
    [Route("[controller]")]
    [Authorize]
    public class UsageController(INcpRepository ncpRepository,
                                 IMapper mapper,
                                 AcmeGwClient gwClient,
                                 CurrencyConverter currencyConverter) : ControllerBase
    {
        [HttpGet("/usages")]
        [AuthorizeAccountIdFilter([nameof(request.AccountIds)])]
        public async Task<ActionResult<PaginationResponse<UsageResponse>>> GetList([FromQuery] UsageRequest request)
        {
            if (request.From.CompareTo(request.To) > 0)
                return BadRequest($"검색 기간 설정 오류 : {request.From} ~ {request.To}");

            if (request.UsageUnit == UsageUnit.Daily &&
                request.From.AddMonths(3).CompareTo(request.To) < 0)
                return BadRequest($"검색 기간 설정 오류 - 일단위 조회는 최대 3개월만 가능합니다 : {request.From} ~ {request.To}");

            if (!string.IsNullOrEmpty(request.AccountIds) && !string.IsNullOrWhiteSpace(request.AccountIds))
            {
                try
                {
                    request.AccountIds
                        .Split(",", StringSplitOptions.TrimEntries)
                        .Select(s =>
                        {
                            if (long.TryParse(s, out long value))
                                return value;
                            throw new ArgumentException();
                        });
                }
                catch (Exception ex)
                {
                    return BadRequest("accountIds는 csv 포맷으로 구성된 정수형 목록입니다");
                }
            }

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={request.AccountIds}", token?.RawData!);
            var accountLinks = await gwClient.Get<List<AccountLink>>($"sales/accountLink?limit=999999&accountIdCsv={request.AccountIds}", token?.RawData!);

            var usages = ncpRepository.GetList(accountLinks.Select(a => a.LinkKey),
                                               request.From,
                                               request.To,
                                               request.Offset,
                                               request.Limit);

            ///ncp 사용 금액은 memberNo 별, 월 기준으로 수집 시점까지 누적된 금액이다.
            ///일 사용 금액을 추정하기 위해 대상일 사용액에서 전일 사용액을 뺀다.
            var groupByMemberNo = usages.GroupBy(u => u.MemberNo);
            foreach (var itemByMember in groupByMemberNo)
            {
                var groupByMonth = itemByMember.GroupBy(m => m.WriteDate.ToString("yyyy-MM"));
                foreach (var itemByMonth in groupByMonth)
                {
                    var dailyEnumeratorByMonth = itemByMonth.OrderByDescending(i => i.WriteDate).GetEnumerator();

                    NcpMaster lastdateRecord = default!;
                    while(dailyEnumeratorByMonth.MoveNext())
                    {
                        if (lastdateRecord != null)
                            lastdateRecord.UseAmount = lastdateRecord.UseAmount - dailyEnumeratorByMonth.Current.UseAmount;
                        lastdateRecord = dailyEnumeratorByMonth.Current;
                    }
                }
            }


            IEnumerable<IGrouping<string, NcpMaster>> grouped = default!;

            if (request.UsageUnit == UsageUnit.Daily)
                grouped = usages.GroupBy(u => u.WriteDate.Date.ToString("yyyy-MM-dd"));
            else if (request.UsageUnit == UsageUnit.Monthly)
                grouped = usages.GroupBy(u => u.WriteDate.ToString("yyyy-MM"));

            var datas = grouped.Select(g =>
            new UsageResponse
            {
                DateString = g.Key,
                Usage = new Models.Dto.Usage
                {
                    TotalCharge = g.Sum(i => i.UseAmount * Convert.ToDecimal(i.ThisMonthPartnerAppliedExchangeRate) ?? 0),
                    Charges = g.Select(i => mapper.Map<Charge>(i, opt =>
                    {
                        opt.AfterMap((o, c) =>
                        {
                            c.AccountName = accounts.FirstOrDefault(
                                a => a.AccountId == accountLinks.FirstOrDefault(
                                    al => al.LinkKey == i.MemberNo)?.AccountId)?.AccountName ?? string.Empty;
                            c.CurrencySymbol = currencyConverter.GetCurrencyInfo(i.PayCurrencyCode ?? string.Empty)?.CurrencySymbol ?? string.Empty;
                        });
                    }))
                }
            });

            return new PaginationResponse<UsageResponse>(datas,
                                                         datas.Count(),
                                                         request.Offset,
                                                         request.Limit);
        }
    }
}
