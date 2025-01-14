using BillingApiTres.Models.Clients;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace BillingApiTres.Helper
{
    /// <summary>
    /// Service account 계정으로 발급된 토큰을 저장하는 저장소
    /// </summary>
    public class ServiceAccountTokenStorage
    {
        private JwtSecurityToken _jwt = new();
        private GatewayToken _gatewayToken = new();
        private bool _isEmpty = true;

        public string RawData => _jwt.RawData;

        public bool IsExpire => _jwt.ValidTo < DateTime.UtcNow;

        public bool IsEmpty => _isEmpty;

        public void Save(GatewayToken token)
        {
            _isEmpty = false;
            _gatewayToken = token;
            _jwt = new JwtSecurityToken(token.AccessToken);
        }
    }

    public record ServiceAccountRequestBody
    {
        public string Realm { get; init; } = string.Empty;
        [JsonPropertyName("clientId")]
        public string ClientId { get; init; } = string.Empty;
        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; init; } = string.Empty;
    }
}
