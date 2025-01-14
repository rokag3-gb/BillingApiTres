using Billing.Data.Interfaces;
using Billing.Data.Models.Iam;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Billing.EF.Repositories
{
    public class ServiceHierarchyRepository(IAMContext iamContext) : IServiceHierarchyRepository
    {
        public async Task<ServiceHierarchy?> GetParent(long accountId)
        {
            try
            {
                return await iamContext.ServiceHierarchies
                    .Include(s => s.Tenant)
                    .Include(s => s.ServiceHierarchyConfigs)
                    .SingleOrDefaultAsync(s => s.AccountId == accountId);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidDataException($"하나 이상의 MSP와 계약 관계인 어카운트입니다 : accountId - {accountId}");
            }
        }

        public async Task<List<ServiceHierarchy>> GetChild(long parentAccountId)
        {
            return await iamContext.ServiceHierarchies
                .Where(s => s.ParentAccId == parentAccountId)
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .ToListAsync();
        }

        public async Task<List<ServiceHierarchy>> GetChild(List<long> parentAccountIds)
        {
            return await iamContext.ServiceHierarchies
                .Where(s => parentAccountIds.Contains(s.ParentAccId))
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .ToListAsync();
        }

        public async Task<ServiceHierarchy?> Get(long serialNo)
        {
            return await iamContext.ServiceHierarchies
                .Include(s => s.Tenant)
                .Include(s => s.ServiceHierarchyConfigs)
                .FirstOrDefaultAsync(s => s.Sno == serialNo);
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

        public async Task<List<ServiceHierarchy>> All(int? offset, int? limit)
        {
            var query = iamContext.ServiceHierarchies.AsQueryable();

            if (offset.HasValue && limit.HasValue)
                query = query.Skip(offset.Value).Take(limit.Value);

            return await query.ToListAsync();
        }
    }
}
