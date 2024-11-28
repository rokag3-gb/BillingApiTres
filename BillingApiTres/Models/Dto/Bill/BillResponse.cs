using AutoMapper;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(Bill))]
    public record BillResponse
    {
        public long BillId { get; set; }
        public DateTime BillDate { get; set; }
        public string BillMonth => BillDate.ToString("yyyyMM");
        public string? SellerAccountName { get; set; } = string.Empty;
        public string? SellerManageName { get; set; }
        public string BuyerAccountId { get; set; } = string.Empty;
        public string? BuyerAccountName { get; set; }
        public string? BuyerManagerId { get; set; }
        public string? BuyerManageName { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string? StatusName { get; set; }
        public string ConsumptionAccountId { get; set; } = string.Empty;
        public string? ConsumptionAccountName { get; set; }
        public DateTime ConsumptionStartDate { get; set; }
        public DateTime ConsumptionEndDate { get; set; }
        public decimal ConsumptionAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal? TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? CurrencyName { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? Remark { get; set; }
        public DateTime SavedAt { get; set; }
        public string? SaverId { get; set; }
        public string? SaverName { get; set; }
    }
}
