namespace Billing.Data.Models.Bill
{
    public partial class BillItem
    {
        public virtual ICollection<NcpDetail> NcpDetails { get; set; } = new List<NcpDetail>();
    }
}
