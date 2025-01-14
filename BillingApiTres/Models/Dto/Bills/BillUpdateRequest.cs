using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto
{
    public record BillUpdateRequest
    {
        [Required]
        public string StatusCode { get; set; }
        [Required]
        public List<long> BillIds { get; set; }
    }
}
