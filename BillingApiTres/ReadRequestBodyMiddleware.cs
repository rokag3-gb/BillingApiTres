using BillingApiTres.Constants;
using System.Text;

namespace BillingApiTres
{
    /// <summary>
    /// FromBody 특성 실행 이후에는 Httpcontext.Request.Body를 읽을 수 없다.
    /// 요청 본문을 다시 읽어야 하는 필요(http request logging)에 의해 Items에 별도로 저장한다.
    /// </summary>
    public class ReadRequestBodyMiddleware
    {
        private readonly RequestDelegate _next;

        public ReadRequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestBody = string.Empty;
            var request = httpContext.Request;

            if (request == null || request.ContentLength == 0)
                await _next(httpContext);

            httpContext.Request.EnableBuffering();
            using var reader = new StreamReader(httpContext.Request.Body, encoding: Encoding.UTF8);
            requestBody = await reader.ReadToEndAsync();
            httpContext.Request.Body.Position = 0;

            httpContext.Items[HttpConstants.RequestBody] = requestBody;

            await _next(httpContext);
        }
    }
}
