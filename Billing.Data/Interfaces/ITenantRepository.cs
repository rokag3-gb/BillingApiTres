using Billing.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Interfaces
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetList(int offset, int limit);
        IAsyncEnumerable<Tenant> GetListAsync(int offset, int limit);
    }
}
