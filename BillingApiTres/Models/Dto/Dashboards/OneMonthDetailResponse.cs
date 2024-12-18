namespace BillingApiTres.Models.Dto.Dashboards
{
    public record OneMonthDetailResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
    }
}
