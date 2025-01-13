using Billing.Data.Models.Iam;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Data.Models.Bill
{
    public partial class Bill
    {
        [NotMapped]
        public long OriginalBillId { get; set; }
        [NotMapped]
        public ICollection<ServiceHierarchyConfig> ServiceHierarchyConfigs { get; set; } = new List<ServiceHierarchyConfig>();
    }
}
