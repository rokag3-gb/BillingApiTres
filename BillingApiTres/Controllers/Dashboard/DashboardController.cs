using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Models.Dto.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingApiTres.Extensions;
using Billing.Data.Models.Bill;
using System.Collections.Generic;

namespace BillingApiTres.Controllers.Dashboard
{
    [Route("[controller]")]
    [Authorize]
    public class DashboardController(IBillRepository billRepository,
                                     ILogger<DashboardController> logger,
                                     IMapper mapper,
                                     IConfiguration config,
                                     ITimeZoneConverter timeZoneConverter) : ControllerBase
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
                var currentSum = current.Sum(d => d.Amount);
                var prevSum = prevMonthBills?.Sum(p => p.Amount) ?? 0;
                var keyDate = new DateTime(current.Key.Year, current.Key.Month, 1);

                result.Add(
                    new RecentThreeMonthResponse
                    {
                        YearMonth = keyDate.ToString("yyyy.MM"),
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
    }
}
