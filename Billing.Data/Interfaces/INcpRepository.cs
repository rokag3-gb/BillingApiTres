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
        List<NcpMaster> GetLatestMasters(IEnumerable<string> keyIds, bool isNoTracking = false);
        List<NcpDetail> GetNcpDetails(IEnumerable<string> keyIds, bool isNoTracking = false);
        HashSet<NcpMarginExceptProduct> GetMarginExceptProducts();
        List<NcpDetail> GetLatestNcpDetails(IEnumerable<string> keyIds, IEnumerable<NcpMarginExceptProduct>? exceptProduct = null);
    }
}
