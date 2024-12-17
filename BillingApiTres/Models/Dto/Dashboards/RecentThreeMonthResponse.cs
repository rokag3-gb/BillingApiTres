namespace BillingApiTres.Models.Dto.Dashboards
{
    public record RecentThreeMonthResponse
    {
        /// <summary>
        /// 청구월
        /// </summary>
        public string YearMonth { get; set; }

        /// <summary>
        /// 청구 기간  from
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 청구 기간 to
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 청구액
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 증감액
        /// </summary>
        public decimal FluctuationAmount { get; set; }

        /// <summary>
        /// 증감율
        /// </summary>
        public decimal FluctuationRate { get; set; }
    }
}
