using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;
using MsEF = Microsoft.EntityFrameworkCore.EF;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.Linq.Expressions;

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

        public List<NcpMaster> GetLatestMasters(IEnumerable<string> keyIds, bool isNoTracking = false)
        {
            var query = billContext.NcpMasters
                .Where(n => keyIds.Contains(n.KeyId));

            query = query.GroupBy(n => n.KeyId).Select(g => g.OrderByDescending(m => m.BatchDate).First());

            if (isNoTracking)
                query = query.AsNoTracking();

            return query.ToList();
        }

        public List<NcpDetail> GetNcpDetails(IEnumerable<string> keyIds, bool isNoTracking = false)
        {
            var query = billContext.NcpDetails
                .Where(n => keyIds.Contains(n.KeyId));

            if (isNoTracking)
                query = query.AsNoTracking();

            return query.ToList();
        }

        public List<NcpDetail> GetLatestNcpDetails(
            IEnumerable<string> keyIds,
            IEnumerable<NcpMarginExceptProduct>? exceptProduct = null)
        {
            var includeNames = exceptProduct?.Where(p => p.Operator == "include").Select(p => $"%{p.ProductName}%") ?? [];
            var excludeNames = exceptProduct?.Where(p => p.Operator == "exclude").Select(p => $"%{p.ProductName}%") ?? [];

            var query = from nd in billContext.NcpDetails
                       join latestDates in (
                           from nd2 in billContext.NcpDetails
                           where keyIds.Contains(nd2.KeyId)
                           group nd2 by nd2.KeyId into g
                           select new
                           {
                               KeyId = g.Key,
                               LatestDate = g.Max(i => i.BatchDate)
                           })
                       on new { C1 = nd.KeyId, C2 = nd.BatchDate }
                       equals new { C1 = latestDates.KeyId, C2 = latestDates.LatestDate }
                       where keyIds.Contains(nd.KeyId)
                            && (includeNames.Any(n => MsEF.Functions.Like(nd.DemandTypeDetailCodeName, n)) == true
                                && excludeNames.Any(n => MsEF.Functions.Like(nd.DemandTypeDetailCodeName, n)) == false)
                       select nd;

            return query
                .AsNoTracking()
                .ToList();
        }

        public HashSet<NcpMarginExceptProduct> GetMarginExceptProducts()
        {
            var query = billContext.NcpMarginExceptProducts
                .Where(n => n.IsActive == true)
                .AsNoTracking();

            return query.ToHashSet();
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
