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
                using var responseStream = await response.Content.ReadAsStreamAsync();

                T obj = default!;
                obj = JsonSerializer.Deserialize<T>(responseStream)!;

                return obj!;
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Sales API에서 코드 값을 가져오지 못했습니다.");
                return default!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"지정된 형식{typeof(T).ToString()}로 역직렬화하지 못했습니다.");
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

            try
            {
                response.EnsureSuccessStatusCode();
                using var responseStream = await response.Content.ReadAsStreamAsync();

                StreamReader sr = new StreamReader(responseStream);
                return await sr.ReadToEndAsync();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"외부 API({combinedUri})에서 값을 가져오지 못했습니다.");
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
