using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Clients
{
    public class AcmeGwClient(HttpClient httpClient,
                              ServiceAccountTokenClient serviceAccountTokenClient,
                              ILogger<AcmeGwClient> logger)
    {
        private readonly string _resource = "sales";

        /// <summary>
        /// 설정된 URI로 Get 동작을 수행합니다
        /// </summary>
        public async Task<T> Get<T>(string uri, string token)
        {
            var combinedUri = string.Join("/", new[] { httpClient.BaseAddress?.ToString().TrimEnd('/') }.Concat(new[] { uri }.Select(s => s.Trim('/'))));

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(combinedUri));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.SendAsync(request, CancellationToken.None);

            try
            {
                response.EnsureSuccessStatusCode();

                T obj = await response.Content.ReadFromJsonAsync<T>() ?? default!;
                return obj!;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"외부 API({combinedUri}) 호출 에러");

                if (ex.StatusCode.HasValue && (int)ex.StatusCode >= 400 && (int)ex.StatusCode < 500)
                {
                    var innerMessage = await response.Content.ReadAsStringAsync();
                    throw new BadHttpRequestException($"{ex.Message} -- {innerMessage}");
                }

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return default!;
            }
        }

        /// <summary>
        /// service account를 사용하여 직렬화 문자열을 반환하는 Get 동작을 수행합니다.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSerializedString(string uri, HttpMethod httpMethod, JsonContent? jsonContent = null)
        {
            var combinedUri = string.Join("/", new[] { httpClient.BaseAddress?.ToString().TrimEnd('/') }.Concat(new[] { uri }.Select(s => s.Trim('/'))));

            using var request = new HttpRequestMessage(httpMethod, new Uri(combinedUri));
            if (jsonContent != null)
                request.Content = jsonContent;
            var serviceAccountToken = await serviceAccountTokenClient.GetToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceAccountToken);

            var response = await httpClient.SendAsync(request, CancellationToken.None);
            var content = string.Empty;

            try
            {
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"외부 API({combinedUri})에서 값을 가져오지 못했습니다.");

                if (ex.StatusCode.HasValue && (int)ex.StatusCode >= 400 && (int)ex.StatusCode < 500)
                    throw new BadHttpRequestException($"{ex.Message} -- {content}");

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"외부 API({combinedUri}) 요청 오류");
                throw;
            }
        }
    }

    public record SalesAccount
    {
        [JsonPropertyName("accountId")]
        public long AccountId { get; set; }
        [JsonPropertyName("accountName")]
        public string AccountName { get; set; }
        [JsonPropertyName("managerId")]
        public string ManagerId { get; set; }
    }

    public record SaleCode
    {
        [JsonPropertyName("codeKey")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public record IamUserEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("username")]
        public string Name { get; set; }
    }

    public record AccountUser
    {
        [JsonPropertyName("accountId")]
        public long AccountId { get; set; }
    }

    public record AccountLink
    {
        [JsonPropertyName("accountId")]
        public long AccountId { get; set; }

        [JsonPropertyName("linkKey")]
        public string LinkKey { get; set; }
    }

    public record GatewayToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }
}
