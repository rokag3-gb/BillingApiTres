using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Data.Models.Bill
{
    public partial class BillItem
    {
        [NotMapped]
        public virtual NcpMaster? NcpMaster { get; set; }
        [NotMapped]
        public virtual ICollection<NcpDetail> NcpDetails { get; set; } = new List<NcpDetail>();
    }
}
