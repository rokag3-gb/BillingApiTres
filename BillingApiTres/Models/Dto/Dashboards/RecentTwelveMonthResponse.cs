namespace BillingApiTres.Models.Dto.Dashboards
{
    public record RecentTwelveMonthResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<AccountAmount> Amounts { get; set; } = new List<AccountAmount>();//
    }

    public record AccountAmount
    {
        public string? AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencySymbol { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
