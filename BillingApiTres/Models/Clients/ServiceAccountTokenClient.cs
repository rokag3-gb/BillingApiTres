using BillingApiTres.Helper;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BillingApiTres.Models.Clients
{
    /// <summary>
    /// service account 계정의 토큰을 발급하는 클라이언트
    /// </summary>
    /// <param name="options">service account 토큰을 발급받기 위한 요청 본문</param>
    public class ServiceAccountTokenClient(IOptions<ServiceAccountRequestBody> options,
                                           ServiceAccountTokenStorage serviceAccountTokenStorage,
                                           HttpClient httpClient)
    {
        public async Task<string> GetToken()
        {
            if (serviceAccountTokenStorage.IsEmpty || serviceAccountTokenStorage.IsExpire)
            {
                var token = await RequestTokenAsync();
                if (token != null)
                    serviceAccountTokenStorage.Save(token);
            }

            return serviceAccountTokenStorage.RawData;
        }

        private async Task<GatewayToken?> RequestTokenAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{httpClient.BaseAddress}token"));
            request.Content = JsonContent.Create(options.Value);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            StreamReader sr = new StreamReader(responseStream);
            return JsonSerializer.Deserialize<GatewayToken>(responseStream);
        }
    }
}
