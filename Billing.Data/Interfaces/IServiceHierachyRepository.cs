using Billing.Data.Models.Iam;

namespace Billing.Data.Interfaces
{
    public interface IServiceHierarchyRepository
    {
        Task<ServiceHierarchy> Add(ServiceHierarchy entity);
        bool CheckInvalidation(long parentAccountId, long accountId);
        Task Delete(ServiceHierarchy entity);
        Task<ServiceHierarchy?> Get(long serialNo, bool? isActive = null);
        Task<List<ServiceHierarchy>> GetChild(long parentAccountId, bool? isActive = null);
        Task<List<ServiceHierarchy>> GetChild(List<long> parentAccountIds, bool? isActive = null);
        List<ServiceHierarchy> GetList(IEnumerable<long>? accountIds, IEnumerable<string>? typeCodes, bool? isActive = null, int? offset = null, int? limit = null);
        Task<ServiceHierarchy?> GetParent(long accountId, bool? isActive = null);
        Task Update(ServiceHierarchy entity);
    }
}
