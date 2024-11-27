using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics;
using TimeZoneConverter;

namespace BillingApiTres
{
    public class TimezoneHeaderCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string? _timezoneHeaderName;

        public TimezoneHeaderCheckMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _timezoneHeaderName = config.GetValue<string>("TimezoneHeader");
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrEmpty(_timezoneHeaderName) || string.IsNullOrWhiteSpace(_timezoneHeaderName))
                throw new BadHttpRequestException("timezone header의 이름을 구성하지 않았습니다");

            var timeZoneHeader = context.Request.Headers[_timezoneHeaderName];

            if (string.IsNullOrEmpty(timeZoneHeader) || string.IsNullOrWhiteSpace(timeZoneHeader))
                throw new BadHttpRequestException("Timezone header가 없습니다");

            if (TZConvert.TryGetTimeZoneInfo(timeZoneHeader!, out var tzInfo) == false)
                throw new BadHttpRequestException($"적절하지 않은 timezone id입니다 : {timeZoneHeader}");

            await _next(context);
        }
    }

    public static class RequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseTimezoneHeaderChecker(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TimezoneHeaderCheckMiddleware>();
        }
    }
}
