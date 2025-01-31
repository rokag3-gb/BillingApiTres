using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto.Bills
{
    public record BillDeleteRequest
    {
        [Required]
        public List<long> BillIds { get; set; } = default!;
    }
}
