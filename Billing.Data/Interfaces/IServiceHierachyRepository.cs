using Billing.Data.Models.Iam;

namespace Billing.Data.Interfaces
{
    public interface IServiceHierarchyRepository
    {
        Task<ServiceHierarchy> Add(ServiceHierarchy entity);
        Task<List<ServiceHierarchy>> All(int? offset, int? limit);
        Task Delete(ServiceHierarchy entity);
        Task<ServiceHierarchy?> Get(long serialNo);
        Task<List<ServiceHierarchy>> GetChild(long parentAccountId);
        Task<List<ServiceHierarchy>> GetChild(List<long> parentAccountIds);
        List<ServiceHierarchy> GetList(IEnumerable<long>? accountIds = null, IEnumerable<string>? typeCodes = null);
        Task<ServiceHierarchy?> GetParent(long accountId);
        Task Update(ServiceHierarchy entity);
    }
}
