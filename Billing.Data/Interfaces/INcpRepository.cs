using Billing.Data.Models.Bill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Interfaces
{
    public interface INcpRepository
    {
        List<NcpMaster> GetList(IEnumerable<string> memberNos, DateTime from, DateTime to, int? offset, int? limit);
    }
}
