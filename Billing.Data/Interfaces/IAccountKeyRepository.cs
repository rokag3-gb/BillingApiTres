using Billing.Data.Models.Sale;

namespace Billing.Data.Interfaces
{
    public interface IAccountKeyRepository
    {
        Task<AccountKey> GetId(string accountKey);
        Task<List<AccountKey>> GetIdList(List<string> accountKeys);
        Task<List<AccountKey>> GetKeyList(List<long> accountIds);
    }
}
