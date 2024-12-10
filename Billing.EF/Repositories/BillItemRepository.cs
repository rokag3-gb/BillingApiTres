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
                .Where(bi => bi.BillId == billId);

            if (offset.HasValue && limit.HasValue)
                query = query.Skip(offset.Value).Take(limit.Value);

            return query.ToList();
        }
    }
}
