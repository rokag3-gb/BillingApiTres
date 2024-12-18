using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Models.Dto.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingApiTres.Extensions;
using Billing.Data.Models.Bill;
using System.Collections.Generic;
using BillingApiTres.Models.Clients;
using System.Linq;

namespace BillingApiTres.Controllers.Dashboard
{
    [Route("[controller]")]
    [Authorize]
    public class DashboardController(IBillRepository billRepository,
                                     ILogger<DashboardController> logger,
                                     IMapper mapper,
                                     IConfiguration config,
                                     ITimeZoneConverter timeZoneConverter,
                                     AcmeGwClient gwClient) : ControllerBase
    {
        /// <summary>
        /// 요청 월 기준 최근 3개월 청구액 합산을 요청합니다
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("/RecentThreeMonth")]
        public ActionResult<List<RecentThreeMonthResponse>> RecentThreeMonth(
            [FromQuery] RecentThreeMonthRequest request)
        {
            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            var requestFrom = new DateTime(request.RequestDate.Year,
                                           request.RequestDate.Month,
                                           1, 0, 0, 0).AddMonths(-3);

            var requestTo = new DateTime(request.RequestDate.Year,
                                         request.RequestDate.Month,
                                         DateTime.DaysInMonth(request.RequestDate.Year, request.RequestDate.Month),
                                         23, 59, 59);
            requestFrom = timeZoneConverter.ConvertToUtc(requestFrom, tz!);
            requestTo = timeZoneConverter.ConvertToUtc(requestTo, tz!);

            var accountIds = request.AccountIdsCsv
                .Split(",", StringSplitOptions.TrimEntries)
                .Select(s =>
                {
                    if (long.TryParse(s, out long id))
                        return id;
                    return -1;
                })
                .Where(i => i >= 1);

            if (HttpContext.AuthenticateAccountId(accountIds) == false)
                return Forbid();

            var datas = billRepository.GetRange(requestFrom, requestTo, accountIds.ToList(), null, null);
            datas.ForEach(d =>
            {
                d.BillDate = timeZoneConverter.ConvertToLocal(d.BillDate, tz!);
                if (d.ConsumptionStartDate.HasValue)
                    d.ConsumptionStartDate = timeZoneConverter.ConvertToLocal(d.ConsumptionStartDate.Value, tz!);
                if (d.ConsumptionEndDate.HasValue)
                    d.ConsumptionEndDate = timeZoneConverter.ConvertToLocal(d.ConsumptionEndDate.Value, tz!);
            });

            var groupByMonth = datas.GroupBy(d => new { d.BillDate.Year, d.BillDate.Month })
                .OrderBy(g => g.Key)
                .GetEnumerator();

            List<RecentThreeMonthResponse> result = new();

            List<Bill>? prevMonthBills = default;
            while (groupByMonth.MoveNext())
            {
                var current = groupByMonth.Current;
                var currentSum = current.Sum(d => d.Amount * (decimal)d.AppliedExchangeRate);
                var prevSum = prevMonthBills?.Sum(p => p.Amount * (decimal)p.AppliedExchangeRate) ?? 0;

                result.Add(
                    new RecentThreeMonthResponse
                    {
                        Year = current.Key.Year,
                        Month = current.Key.Month,
                        Amount = currentSum,
                        FluctuationAmount = prevSum != 0 ? currentSum - prevSum : 0,
                        FluctuationRate = prevSum != 0 ? Math.Round((currentSum - prevSum) / prevSum * 100, 1) : 0,
                        StartDate = current.Min(c => c.ConsumptionStartDate),
                        EndDate = current.Max(c => c.ConsumptionEndDate)
                    });

                prevMonthBills = current.ToList();
            }

            return result;
        }

        /// <summary>
        /// 최근 12개월의 회사별 청구액 합산을 반환합니다
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("/RecentTwelveMonth")]
        public async Task<ActionResult<List<RecentTwelveMonthResponse>>> RecentTwelveMonth(
            [FromQuery] RecentTwelveMonthRequest request)
        {
            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            var requestFrom = new DateTime(request.RequestDate.Year,
                                           request.RequestDate.Month,
                                           1, 0, 0, 0).AddMonths(-3);

            var requestTo = new DateTime(request.RequestDate.Year,
                                         request.RequestDate.Month,
                                         DateTime.DaysInMonth(request.RequestDate.Year, request.RequestDate.Month),
                                         23, 59, 59);
            requestFrom = timeZoneConverter.ConvertToUtc(requestFrom, tz!);
            requestTo = timeZoneConverter.ConvertToUtc(requestTo, tz!);

            var accountIds = request.AccountIdsCsv
                .Split(",", StringSplitOptions.TrimEntries)
                .Select(s =>
                {
                    if (long.TryParse(s, out long id))
                        return id;
                    return -1;
                })
                .Where(i => i >= 1);

            if (HttpContext.AuthenticateAccountId(accountIds) == false)
                return Forbid();

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999", token?.RawData!);

            var datas = billRepository.GetRange(requestFrom, requestTo, accountIds.ToList(), null, null);

            var group = datas.GroupBy(d => new { d.BillDate.Year, d.BillDate.Month });

            var result = group.Select(g => new RecentTwelveMonthResponse
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Amounts = g.GroupBy(b => b.ConsumptionAccountId)
                           .Select(gb =>
                           new AccountAmount
                           {
                               AccountName = accounts.FirstOrDefault(a => a.AccountId == gb.Key)?.AccountName ?? gb.Key.ToString(),
                               Amount = gb.Sum(i => i.Amount * (decimal)i.AppliedExchangeRate),
                               CurrencyCode = "KSW",
                               CurrencySymbol = "₩"
                           }).ToList()
            });

            return result.ToList();
        }
    }
}
