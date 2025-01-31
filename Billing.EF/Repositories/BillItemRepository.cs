using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.EF.Repositories
{
    public class BillItemRepository(BillContext billContext) : IBillItemRepository
    {
        public List<BillItem> GetList(long billId, int? offset, int? limit)
        {
            var query = billContext.BillItems
                .Include(bi => bi.Product)
                .Include(bi => bi.Bill)
                .Where(bi => bi.IsDelete == false && bi.BillId == billId);

            if (offset.HasValue && limit.HasValue)
                query = query.Skip(offset.Value).Take(limit.Value);

            return query.ToList();
        }

        public IEnumerable<BillItem> GetListVenderDetail(long billId, int? offset, int? limit)
        {
            var ncpBillItems = billContext.BillItems
                .Where(bi => bi.BillId == billId)
                .Where(bi => bi.IsDelete == false)
                .Where(bi => bi.VendorCode == "VEN-NCP")
                .Include(bi => bi.Bill)
                .AsNoTracking()
                .ToHashSet();


            var ncpDetails = GetNcpDetails(
                ncpBillItems?.Where(bi => string.IsNullOrEmpty(bi.KeyId) == false)
                             .Select(bi => bi.KeyId)!);
            
            //다른 벤더사 billItems도 필요함. ncp와 합칠 것.
            

            var detailQuery = ncpDetails;
            ///다른 벤더사 추가될 경우
            ///NcpDetail과 AwsDetail은 형식이 다른데 union?
            ///공통 형식 고려할 것
            //var allDetails = ncpDetails.Union(awsDetails); 

            if (offset.HasValue && limit.HasValue)
                detailQuery = detailQuery.Skip(offset.Value).Take(limit.Value);

            var allDetails = detailQuery.AsNoTracking().ToHashSet();

            var billItems = ncpBillItems?.GroupJoin(
                allDetails,
                bi => bi.KeyId,
                d => d.KeyId,
                (bi, d) =>
                {
                    bi.NcpDetails = d.ToList();
                    return bi;
                });
            
            return billItems ?? Enumerable.Empty<BillItem>();
        }

        private IQueryable<NcpDetail> GetNcpDetails(IEnumerable<string> keyIds)
        {
            var query = billContext.NcpDetails
                .Where(n => keyIds.Contains(n.KeyId));

            var largestDate = query.Max(n => n.BatchDate);

            if (largestDate.HasValue == false)
                return query.Take(0);

            return query.Where(n => n.BatchDate == largestDate);
        }
    }
}
