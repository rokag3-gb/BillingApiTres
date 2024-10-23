using Billing.Data.Models;

namespace Billing.Data.Interfaces
{
    public interface IServiceHierarchyRepository
    {
        Task<ServiceHierarchy> Add(ServiceHierarchy entity);
        Task Delete(long serialNo);
        Task<ServiceHierarchy?> Get(long serialNo);
        Task<List<ServiceHierarchy>> GetChild(long parentAccountId);
        Task<ServiceHierarchy?> GetParent(long accountId);
        Task Update(ServiceHierarchy entity);
    }
}
