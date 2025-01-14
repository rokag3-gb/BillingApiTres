namespace BillingApiTres.Models.Dto
{
    public record BillDetailResponse
    {
        public long BillId { get; set; }
        public long BillItemId { get; set; }
        public long BillDetailId { get; set; }
        public long DetailLineId { get; set; }
        public DateOnly? BatchDate { get; set; }
        public string? KeyId { get; set; }
        public string DemandMonth { get; set; } = null!;
        public string Zone { get; set; } = null!;
        public string Account { get; set; } = null!;
        public string MemberNo { get; set; } = null!;
        public string? MemberName { get; set; }
        public DateTime WriteDate { get; set; }
        public string? RegionCode { get; set; }
        //public string? DemandTypeCode { get; set; }
        public string? DemandTypeCodeName { get; set; }
        //public string? DemandTypeDetailCode { get; set; }
        public string? DemandTypeDetailCodeName { get; set; }
        //public string? ContractMemberNo { get; set; }
        public string? ContractContractNo { get; set; }
        //public string? ContractConjunctionContractNo { get; set; }
        //public string? ContractContractTypeCode { get; set; }
        //public string? ContractContractTypeCodeName { get; set; }
        //public string? ContractContractStatusCode { get; set; }
        //public string? ContractContractStatusCodeName { get; set; }
        public DateTime? ContractContractStartDate { get; set; }
        public DateTime? ContractContractEndDate { get; set; }
        public string? ContractInstanceName { get; set; }
        //public string? ContractRegionCode { get; set; }
        //public string? ContractPlatformTypeCode { get; set; }
        //public string? ContractPlatformTypeCodeName { get; set; }
        //public string? ContractProductListContractProductSequence { get; set; }
        //public string? ContractProductListBeforeContractProductSequence { get; set; }
        //public string? ContractProductListProductCode { get; set; }
        public string? ContractProductListPriceNo { get; set; }
        //public string? ContractProductListPromiseNo { get; set; }
        //public string? ContractProductListInstanceNo { get; set; }
        //public string? ContractProductListProductItemKindCode { get; set; }
        public string? ContractProductListProductItemKindCodeName { get; set; }
        //public string? ContractProductListProductRatingTypeCode { get; set; }
        public string? ContractProductListProductRatingTypeCodeName { get; set; }
        //public string? ContractProductListServiceStatusCode { get; set; }
        public string? ContractProductListServiceStatusCodeName { get; set; }
        public DateTime? ContractProductListServiceStartDate { get; set; }
        public DateTime? ContractProductListServiceEndDate { get; set; }
        //public string? ContractProductListProductSize { get; set; }
        //public string? ContractProductListProductCount { get; set; }
        //public string? ContractProductListUsageList { get; set; }
        public double? UnitUsageQuantity { get; set; }
        public double? PackageUnitUsageQuantity { get; set; }
        public double? TotalUnitUsageQuantity { get; set; }
        //public string? UsageUnitCode { get; set; }
        public string? UsageUnitCodeName { get; set; }
        public double? ProductPrice { get; set; }
        public decimal? UseAmount { get; set; }
        public decimal? PromotionDiscountAmount { get; set; }
        public decimal? EtcDiscountAmount { get; set; }
        public decimal? PromiseDiscountAmount { get; set; }
        public decimal? DemandAmount { get; set; }
        public decimal? MemberPriceDiscountAmount { get; set; }
        public decimal? MemberPromiseDiscountAddAmount { get; set; }
        public string? PayCurrencyCode { get; set; }
        //public string? PayCurrencyCodeName { get; set; }
        //public string? SectionPriceList { get; set; }
        public double? ThisMonthAppliedExchangeRate { get; set; }
        //public string? RequestId { get; set; }
        //public string? ReturnCode { get; set; }
        //public string? ReturnMessage { get; set; }
    }
}
