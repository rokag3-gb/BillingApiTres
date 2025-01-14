namespace BillingApiTres.Models.Dto.Dashboards
{
    public record RecentThreeMonthResponse
    {
        /// <summary>
        /// 청구 년도
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 청구 월
        /// </summary>
        public int Month { get; set; }

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
