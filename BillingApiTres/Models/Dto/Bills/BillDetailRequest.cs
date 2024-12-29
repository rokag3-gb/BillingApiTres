using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Dto
{
    public record BillDetailRequest
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}
