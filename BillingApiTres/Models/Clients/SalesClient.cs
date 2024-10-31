using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Clients
{
    public class SalesClient(HttpClient httpClient, ILogger<SalesClient> logger)
    {
        private readonly string _resource = "sales";

        /// <summary>
        /// 설정된 URI로 Get 동작을 수행합니다
        /// </summary>
        public async Task<T> Get<T>(string uri, string token)
        {
            var combinedUri = string.Join("/", new[] { httpClient.BaseAddress?.ToString().TrimEnd('/') }.Concat(new[] { _resource, uri }.Select(s => s.Trim('/'))));

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
    }

    public record SalesAccount
    {
        [JsonPropertyName("accountId")]
        public int AccountId { get; set; }
        [JsonPropertyName("accountName")]
        public string AccountName { get; set; }
    }
}
