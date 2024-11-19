using Billing.Data.Interfaces;
using Billing.Data.Models.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Billing.EF.Repositories
{
    public class AccountKeyRepository(SaleContext saleContext,
                                      ILogger<AccountKeyRepository> logger) : IAccountKeyRepository
    {
        public async Task<AccountKey> GetId(string accountKey)
        {
            var ret = await saleContext.AccountKeys
                .Where(a => a.AccountKey1 == accountKey)
                .FirstOrDefaultAsync();

            return ret;
        }

        public async Task<List<AccountKey>> GetIdList(List<string> accountKeys)
        {
            var list = await saleContext.AccountKeys
                .Where(a => accountKeys.Contains(a.AccountKey1 ?? string.Empty))
                .ToListAsync();

            return list;
        }
    }
}
