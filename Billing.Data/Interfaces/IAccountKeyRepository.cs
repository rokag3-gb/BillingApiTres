using Billing.Data.Models.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Data.Interfaces
{
    public interface IAccountKeyRepository
    {
        Task<AccountKey> GetId(string accountKey);
        Task<List<AccountKey>> GetIdList(List<string> accountKeys);
    }
}
