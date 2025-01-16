using Billing.Data.Interfaces;
using Billing.Data.Models.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq.Expressions;

namespace Billing.EF.Repositories
{
    public class ServiceHierarchyRepository(IAMContext iamContext) : IServiceHierarchyRepository
    {
        public async Task<ServiceHierarchy?> GetParent(long accountId, bool? isActive)
        {
            try
            {
                var query = iamContext.ServiceHierarchies
                    .Include(s => s.Tenant)
                    .Include(s => s.ServiceHierarchyConfigs)
                    .AsQueryable();

                if (isActive.HasValue)
                    query = query.Where(s => s.IsActive == isActive);

                return await query.SingleOrDefaultAsync(s => s.AccountId == accountId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidDataException($"하나 이상의 MSP와 계약 관계인 어카운트입니다 : accountId - {accountId}");
            }
        }

        public async Task<List<ServiceHierarchy>> GetChild(long parentAccountId, bool? isActive)
        {
            var query = iamContext.ServiceHierarchies
                .Where(s => s.ParentAccId == parentAccountId)
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive);

            return await query.ToListAsync();
        }

        public async Task<List<ServiceHierarchy>> GetChild(List<long> parentAccountIds, bool? isActive)
        {
            var query = iamContext.ServiceHierarchies
                .Where(s => parentAccountIds.Contains(s.ParentAccId))
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive);

            return await query.ToListAsync();
        }

        public async Task<ServiceHierarchy?> Get(long serialNo, bool? isActive)
        {
            var query = iamContext.ServiceHierarchies
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive);

            return await query.FirstOrDefaultAsync(s => s.Sno == serialNo);
        }

        public List<ServiceHierarchy> GetList(IEnumerable<long>? accountIds, IEnumerable<string>? typeCodes, bool? isActive, int? offset, int? limit)
        {
            var query = iamContext.ServiceHierarchies
                .Include(s => s.ServiceHierarchyConfigs)
                .AsQueryable();

            var predicate = PredicateBuilder.False<ServiceHierarchy>();

            if (isActive.HasValue)
                predicate = predicate.And(s => s.IsActive == isActive);

            if (accountIds?.Any() == true)
                predicate = predicate.Or(s => accountIds.Contains(s.AccountId));

            if (typeCodes?.Any() == true)
                predicate = predicate.Or(s => typeCodes.Contains(s.TypeCode));

            if (predicate.Body.NodeType == ExpressionType.Constant &&
                predicate.Body.ToString() == false.ToString())
            {
                predicate = PredicateBuilder.True<ServiceHierarchy>();
            }

            query = query.Where(predicate);

            if (offset.HasValue && limit.HasValue)
                query = query.Skip(offset.Value)
                             .Take(limit.Value);

            return query.ToList();
        }

        public bool CheckInvalidation(long parentAccountId, long accountId)
        {
            ///중복 / 이중 계약 또는 순환 관계
            if (iamContext.ServiceHierarchies.Any(s => s.AccountId == accountId
                                                       || (s.ParentAccId == accountId && s.AccountId == parentAccountId)))
                return true;

            var parent = iamContext.ServiceHierarchies.FirstOrDefault(s => s.AccountId == parentAccountId);
            ///잘못된 부모 설정
            if (parent == null || parent.TypeCode == "SHT-CUS")
                return true;

            return false;
        }

        public async Task Update(ServiceHierarchy entity)
        {
            using var trans = await iamContext.Database.BeginTransactionAsync();

            iamContext.ServiceHierarchies.Update(entity);
            await iamContext.SaveChangesAsync();

            await trans.CommitAsync();
        }

        public async Task<ServiceHierarchy> Add(ServiceHierarchy entity)
        {
            entity.Tenant = iamContext.Tenants.Single(t => t.TenantId == entity.TenantId);
            var added = iamContext.ServiceHierarchies.Add(entity);
            await iamContext.SaveChangesAsync();

            return added.Entity;
        }

        public async Task Delete(ServiceHierarchy entity)
        {
            iamContext.Remove(entity);
            await iamContext.SaveChangesAsync();
        }
    }
}
