using Billing.Data.Models;

namespace Billing.Data.Interfaces
{
    public interface IServiceHierarchyRepository
    {
        Task Delete(long serialNo);
        Task<ServiceHierarchy?> Get(int serialNo);
        Task<List<ServiceHierarchy>> GetChild(long parentAccountId);
        Task<ServiceHierarchy?> GetParent(long accountId);
        Task Update(ServiceHierarchy entity);
    }
}
