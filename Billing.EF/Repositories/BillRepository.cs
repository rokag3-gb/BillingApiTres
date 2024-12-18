using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Billing.EF.Repositories
{
    public class BillRepository(BillContext billContext, ILogger<BillRepository> logger) : IBillRepository
    {
        public List<Bill> GetRange(DateTime from, DateTime to, List<long> accountIds, int? offset, int? limit)
        {
            List<Bill> bills = new();
            var query = billContext.Bills
                .Where(b => accountIds.Contains(b.BuyerAccountId) || accountIds.Contains(b.SellerAccountId))
                .Where(b => b.BillDate >= from && b.BillDate <= to);

            if (offset != null && limit != null)
                bills = query.Skip(offset.Value).Take(limit.Value).ToList();
            else
                bills = query.ToList();

            return bills;
        }

        public List<Bill> GetRangeWithRelations(DateTime from, DateTime to, List<long> accountIds, int? offset, int? limit)
        {
            var query = billContext.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.BillDetails)
                .Where(b => accountIds.Contains(b.BuyerAccountId) || accountIds.Contains(b.SellerAccountId))
                .Where(b => b.BillDate >= from && b.BillDate <= to);

            if (offset != null && limit != null)
                query = query.Skip(offset.Value).Take(limit.Value);
            
            return query.ToList();
        }
    }
}
