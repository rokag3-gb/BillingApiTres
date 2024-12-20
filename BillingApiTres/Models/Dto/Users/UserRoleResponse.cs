using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto.Users
{
    public record UserRoleResponse
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("roles")]
        public List<RoleDto> Roles { get; set; } = new();
    }

    public record RoleDto
    {
        [JsonPropertyName("roleId")]
        public string RoleId { get; set; } = string.Empty;
        [JsonPropertyName("roleName")]
        public string RoleName { get; set; } = string.Empty;
    }
}
