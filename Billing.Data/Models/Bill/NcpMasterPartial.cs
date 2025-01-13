using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Data.Models.Bill
{
    public partial class NcpMaster
    {
        //[ForeignKey("KeyId")]
        //[InverseProperty("NcpMasters")]
        [NotMapped]
        public virtual BillItem BillItem { get; set; } = null!;
    }
}
