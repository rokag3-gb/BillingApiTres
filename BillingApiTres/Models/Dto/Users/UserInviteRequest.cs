using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto.Users
{
    public record UserInviteRequest
    {
        [Required]
        [JsonPropertyName("accountId")]
        public long AccountId { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("senderEmail")]
        public string SenderEmail { get; set; } = string.Empty;
    }
}
