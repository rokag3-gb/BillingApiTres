using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;

namespace Billing.EF.Repositories
{
    public class NcpRepository(BillContext billContext) : INcpRepository
    {
        public List<NcpMaster> GetList(IEnumerable<string> memberNos,
                                       DateTime from,
                                       DateTime to,
                                       int? offset,
                                       int? limit)
        {
            var query = billContext.NcpMasters
                .Where(n => memberNos.Contains(n.MemberNo))
                .Where(n => n.WriteDate.Date.AddDays(-1) >= from)
                .Where(n => n.WriteDate.Date.AddDays(-1) <= to);

            if (offset.HasValue && limit.HasValue) 
                query = query.Skip(offset.Value).Take(limit.Value);

            query = query.OrderBy(n => n.MemberNo).ThenBy(n => n.WriteDate);

            query = query.Select(n => n.AdjustWriteDate());

            return query.ToList();
        }
    }

    public static class NcpMasterExtension
    {
        public static NcpMaster AdjustWriteDate(this NcpMaster ncpMaster)
        {
            ncpMaster.WriteDate = ncpMaster.WriteDate.AddDays(-1);
            return ncpMaster;
        }
    }
}
