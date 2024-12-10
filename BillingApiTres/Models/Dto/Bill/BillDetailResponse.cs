using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto
{
    public record BillDetailResponse
    {
        public long BillId { get; set; }
        public long BillItemId { get; set; }
        public long BillDetailId { get; set; }
        public string? KeyId { get; set; }
        public long DetailLineId { get; set; }
        public string? DemandType { get; set; }
        public string? DemandTypeDetail { get; set; }
        public double? UnitUsageQuantity { get; set; }
    }
}
