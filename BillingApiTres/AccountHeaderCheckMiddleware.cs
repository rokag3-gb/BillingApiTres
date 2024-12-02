using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using TimeZoneConverter;

namespace BillingApiTres
{
    /// <summary>
    /// 요청 헤더에 적합한 Account Header가 포함되었는지 검사하는 미들웨어를 정의합니다.
    /// </summary>
    public class AccountHeaderCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string? _accountHeaderName;
        private ILogger<AccountHeaderCheckMiddleware> _logger;

        public AccountHeaderCheckMiddleware(RequestDelegate next,
                                            IConfiguration config,
                                            ILogger<AccountHeaderCheckMiddleware> logger)
        {
            _next = next;
            _accountHeaderName = config.GetValue<string>("AccountHeader");
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrEmpty(_accountHeaderName) || string.IsNullOrWhiteSpace(_accountHeaderName))
            {
                _logger.LogError("X-Account 헤더가 없습니다.");
                throw new BadHttpRequestException("필요한 헤더가 구성되지 않았습니다. GW 관리자에게 문의하세요");
            }

            var accountHeader = context.Request.Headers[_accountHeaderName];

            if (string.IsNullOrEmpty(accountHeader) || string.IsNullOrWhiteSpace(accountHeader))
            {
                _logger.LogError("X-Account 헤더의 값이 비었습니다.");
                throw new BadHttpRequestException("필요한 헤더가 구성되지 않았습니다. GW 관리자에게 문의하세요");
            }

            var accountIds = accountHeader
                .ToString()
                .Split(",", StringSplitOptions.TrimEntries)
                .Select(s => 
                {
                    if (long.TryParse(s, out long id))
                        return id;
                    return -1;
                })
                .Where(i => i >= 1)
                .ToImmutableHashSet();

            context.Items.Add(_accountHeaderName, accountIds);

            await _next(context);
        }
    }

    public static class RequestAccountMiddlewareExtensions
    {
        public static IApplicationBuilder UseAccountHeaderChecker(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AccountHeaderCheckMiddleware>();
        }
    }
}
