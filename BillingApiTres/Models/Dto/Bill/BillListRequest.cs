using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto
{
    public record BillListRequest
    {
        [Required]
        public DateTime From { get; set; }

        [Required]
        public DateTime To { get; set; }

        [Required]
        public List<string> AccountIds { get; set; }

        [Range(0, int.MaxValue)]
        public int? Offset { get; set; }

        [Range(0, int.MaxValue)]
        public int? limit { get; set; }
    }
}
