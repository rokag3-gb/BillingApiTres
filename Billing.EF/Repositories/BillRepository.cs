using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Billing.Data.Models.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;

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

        public List<Bill> GetLatestPublishedBill(IEnumerable<(DateTime BillDate, long BuyerAccountId, long? ConsumptionAccountId)> conditions)
        {
            //var query = billContext.Bills
            //    .Where(b => conditions.Any(c => b.BillDate == c.billdate
            //                                    && b.BuyerAccountId == c.buyerAccountId
            //                                    && b.ConsumptionAccountId == c.comsumptionAccountId));

            var query = billContext.Bills.AsQueryable();
            var predicate = PredicateBuilder.False<Bill>();

            foreach (var condition in conditions)
            {
                predicate = predicate.Or(b => 
                    b.BillDate == condition.BillDate
                    && b.BuyerAccountId == condition.BuyerAccountId
                    && b.ConsumptionAccountId == condition.ConsumptionAccountId);
            }

            query = query.Where(predicate);

            query = query.GroupBy(b => new { b.BillDate, b.BuyerAccountId, b.ConsumptionAccountId })
                         .Select(g => g.OrderByDescending(b => b.SavedAt).First());

            return query.ToList();
        }

        public List<Bill> GetRangeWithRelations(DateTime? from,
                                                DateTime? to,
                                                List<long>? accountIds,
                                                List<long>? billIds,
                                                int? offset,
                                                int? limit,
                                                bool isNoTracking = false)
        {
            var query = billContext.Bills
                .Include(b => b.BillItems)
                .ThenInclude(bi => bi.Product)
                .AsQueryable();

            if (accountIds?.Any() == true)
                query = query.Where(b => accountIds.Contains(b.BuyerAccountId) || accountIds.Contains(b.SellerAccountId));
            if (billIds?.Any() == true)
                query = query.Where(b => billIds.Contains(b.BillId));

            if (from.HasValue && to.HasValue)
                query = query.Where(b => b.BillDate >= from.Value && b.BillDate <= to.Value);

            if (offset != null && limit != null)
                query = query.Skip(offset.Value).Take(limit.Value);

            if (isNoTracking)
                query = query.AsNoTracking();

            return query.ToList();
        }

        public int UpdateStatus(string statusCode, IEnumerable<long> billIds)
        {
            var count = billContext.Bills
                .Where(b => billIds.Contains(b.BillId))
                .ExecuteUpdate(b => b.SetProperty(p => p.StatusCode, statusCode));

            return count;
        }

        public List<Bill> Create(IEnumerable<Bill> bills)
        {
            bills.SelectMany(b => b.BillItems)
                 .ToList()
                 .ForEach(bi =>
                 {
                     bi.ProductId = bi.Product?.ProductId;
                     bi.Product = null;
                 });

            billContext.Bills.AddRange(bills);

            var count = billContext.SaveChanges();

            return bills.Where(b => b.BillId > 0).ToList();
        }
    }
}
