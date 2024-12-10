using Billing.Data.Models.Bill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Interfaces
{
    public interface IBillItemRepository
    {
        List<BillItem> GetList(long billId, int? offset, int? limit);
    }
}
