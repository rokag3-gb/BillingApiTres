using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(BillItem))]
    public class BillItemResponse
    {
        [SourceMember(nameof(BillItem.BillId))]
        public long BillId { get; set; }

        [SourceMember(nameof(BillItem.BillItemId))]
        public long BillItemId { get; set; }

        [SourceMember("Product.ProductId")]
        public long ProductId { get; set; }

        [SourceMember("Product.ProductName")]
        public string ProductName { get; set; }

        [SourceMember(nameof(BillItem.Amount))]
        public double Amount { get; set; }

        [SourceMember(nameof(BillItem.Description))]
        public string Description { get; set; }
    }
}
