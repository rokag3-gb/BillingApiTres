using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto.Users
{
    public record UserChangePasswordRequest
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordConfirm { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
