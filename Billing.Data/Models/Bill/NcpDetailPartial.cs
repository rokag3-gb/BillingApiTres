using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Models.Bill
{
    public partial class NcpDetail
    {
        [ForeignKey("KeyId")]
        [InverseProperty("NcpDetails")]
        public virtual BillItem BillItem { get; set; } = null!;
    }
}
