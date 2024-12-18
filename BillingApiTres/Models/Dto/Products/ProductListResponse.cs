using AutoMapper;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto.Products
{
    [AutoMap(typeof(Product))]
    public record ProductListResponse
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public string SaverId { get; set; } = string.Empty;
    }
}
