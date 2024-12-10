using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.EF.Repositories
{
    public class BillDetailRepository(BillContext billContext) : IBillDetailRepository
    {
        public List<BillDetail> GetList(long billId, int? offset, int? limit)
        {
            var ncpDetails = GetDetailsWithNcp(billId);
            //var awsDetails = GetDetailsWithAws(billId);

            var allDetails = ncpDetails;
            //다른 벤더사 추가될 경우
            //var allDetails = ncpDetails.Union(awsDetails);

            if (offset.HasValue && limit.HasValue)
                allDetails = allDetails.Skip(offset.Value).Take(limit.Value);

            return allDetails.ToList();
        }

        private IQueryable<BillDetail> GetDetailsWithNcp(long billId)
        {
            return billContext.BillItems.Where(bi => bi.BillId == billId)
                .Include(bi => bi.Bill)
                .Include(bi => bi.BillDetails.Where(d => d.VendorCode == "VEN-NCP"))
                .ThenInclude(d => d.NcpDetails)
                .SelectMany(bi => bi.BillDetails)
                .Include(bd => bd.BillItem)
                .ThenInclude(bi => bi.Bill)
                .Where(bd => bd.NcpDetails.Any());
        }

        //NCP 아닌 다른 벤더(AWS)의 경우
        //private IQueryable<BillDetail> GetDetailsWithAws(long billId)
        //{
        //    return billContext.BillItems.Where(bi => bi.BillId == billId)
        //        .Include(bi => bi.Bill)
        //        .Include(bi => bi.BillDetails.Where(d => d.VendorCode == "VEN-AWS"))
        //        .ThenInclude(d => d.AwsDetails)
        //        .SelectMany(bi => bi.BillDetails)
        //        .Include(bd => bd.BillItem)
        //        .ThenInclude(bi => bi.Bill)
        //        .Where(bd => bd.AwsDetails.Any());
        //}
    }
}
