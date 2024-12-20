using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto
{
    public record AccountUserResponse
    {
        [JsonPropertyName("seq")]
        public long? Seq { get; set; }
        [JsonPropertyName("accountId")]
        public long AccountId { get; set; }
        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; } = null!;
        [JsonPropertyName("userId")]
        public string? UserId { get; set; } = null!;
        [JsonPropertyName("userInfo")]
        public User? UserInfo { get; set; }
        [JsonPropertyName("isUse")]
        public bool? IsUse { get; set; }
        [JsonPropertyName("saveDate")]
        public DateTime? SaveDate { get; set; }
        [JsonPropertyName("saveId")]
        public string? SaveId { get; set; }
        [JsonPropertyName("saver")]
        public User? Saver { get; set; }
        public IEnumerable<BillRoleResponse>? Roles { get; set; }
    }

    public record User(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("createdTimestamp")] long CreatedTimestamp,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("requiredActions")] string[] RequiredActions,
    [property: JsonPropertyName("createDate")] string CreateDate,
    [property: JsonPropertyName("creator")] string Creator,
    [property: JsonPropertyName("modifyDate")] string ModifyDate,
    [property: JsonPropertyName("modifier")] string Modifier
);
}
