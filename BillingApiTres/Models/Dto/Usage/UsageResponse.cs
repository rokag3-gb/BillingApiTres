using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto
{
    public record UsageResponse
    {
        public string DateString { get; set; }

        public Usage Usage { get; set; }
    }

    public record Usage
    {
        public decimal TotalCharge { get; set; }
        public IEnumerable<Charge> Charges { get; set; }
    }

    [AutoMap(typeof(NcpMaster))]
    public record Charge
    {
        /// <summary>
        /// 사용일
        /// </summary>
        [SourceMember(nameof(NcpMaster.WriteDate))]
        public DateTime ChargeDate { get; set; }

        /// <summary>
        /// 회원번호
        /// </summary>
        [SourceMember(nameof(NcpMaster.MemberNo))]
        public string MemberNo { get; set; }

        /// <summary>
        /// 고객사명
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 청구대상월
        /// </summary>
        [SourceMember(nameof(NcpMaster.DemandMonth))]
        public string DemandMonth { get; set; }

        /// <summary>
        /// 사용 금액
        /// </summary>
        [SourceMember(nameof(NcpMaster.UseAmount))]
        public decimal useAmount { get; set; }

        [SourceMember(nameof(NcpMaster.ThisMonthPartnerAppliedExchangeRate))]
        public decimal thisMonthPartnerAppliedExchangeRate { get; set; }

        public decimal useAmountKrw => useAmount * thisMonthPartnerAppliedExchangeRate;

        /// <summary>
        /// 통화 기호
        /// </summary>
        public string CurrencySymbol { get; set; }
    }
}
