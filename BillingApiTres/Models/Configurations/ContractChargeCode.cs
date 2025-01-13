namespace BillingApiTres.Models.Configurations
{
    public record ContractChargeCode
    {
        public string NcpPcpDiscount { get; set; } = "PcpContractDiscountRate";
        public string NcpWorkDiscount { get; set; } = "WorksContractDiscountRate";
        public string MspCharge { get; set; } = "MspChargeRate";
    }
}
