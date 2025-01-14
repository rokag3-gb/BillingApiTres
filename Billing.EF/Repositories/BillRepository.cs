using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Billing.EF.Repositories
{
    public class BillRepository(BillContext billContext, ILogger<BillRepository> logger) : IBillRepository
    {
        public List<Bill> GetRange(DateTime? from, DateTime? to, List<long>? accountIds, List<long>? billIds, int? offset, int? limit)
        {
            List<Bill> bills = new();
            var query = billContext.Bills.AsQueryable();

            if (from.HasValue && to.HasValue)
                query = query.Where(b => b.BillDate >= from.Value && b.BillDate <= to.Value);

            if (accountIds?.Any() == true)
                query = query.Where(b => accountIds.Contains(b.BuyerAccountId) || accountIds.Contains(b.SellerAccountId));

            if (billIds?.Any() == true)
                query = query.Where(b => billIds.Contains(b.BillId));

            if (offset != null && limit != null)
                query = query.Skip(offset.Value).Take(limit.Value);

            query.OrderBy(b => b.BillDate);

            return query.ToList();
        }

        public List<Bill> GetRangeWithRelations(DateTime from, DateTime to, List<long> accountIds, int? offset, int? limit)
        {
            var query = billContext.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .Where(b => accountIds.Contains(b.BuyerAccountId) || accountIds.Contains(b.SellerAccountId))
                .Where(b => b.BillDate >= from && b.BillDate <= to);

            if (offset != null && limit != null)
                query = query.Skip(offset.Value).Take(limit.Value);

            return query.ToList();
        }

        public int UpdateStatus(string statusCode, IEnumerable<long> billIds)
        {
            var count = billContext.Bills
                .Where(b => billIds.Contains(b.BillId))
                .ExecuteUpdate(b => b.SetProperty(p => p.StatusCode, statusCode));

            return count;
        }
    }
}
