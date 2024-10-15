using Billing.Data.Interfaces;
using Billing.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.EF.Repositories
{
    public class TenantRepository(IAMContext iamContext) : ITenantRepository
    {
        public IEnumerable<Tenant> GetList(int offset, int limit)
        {
            return iamContext.Tenants
                .Where(t => t.IsActive == true)
                .Skip(offset)
                .Take(limit)
                .AsEnumerable();
        }

        public IAsyncEnumerable<Tenant> GetListAsync(int offset, int limit)
        {
            return iamContext.Tenants
                .Where(t => t.IsActive == true)
                .Skip(offset)
                .Take(limit)
                .AsAsyncEnumerable();
        }
    }
}
