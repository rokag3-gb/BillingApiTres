using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto.Authority
{
    public record AuthorityPostRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
