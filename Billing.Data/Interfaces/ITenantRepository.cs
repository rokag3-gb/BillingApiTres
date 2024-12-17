using Billing.Data.Models.Iam;

namespace Billing.Data.Interfaces
{
    public interface ITenantRepository
    {
        IEnumerable<Tenant> GetList(int offset, int limit);
        IAsyncEnumerable<Tenant> GetListAsync(int offset, int limit);
    }
}
