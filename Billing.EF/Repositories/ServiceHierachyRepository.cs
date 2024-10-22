using Billing.Data.Interfaces;
using Billing.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .AsNoTracking()                    
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
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ServiceHierarchy?> Get(int serialNo)
        {
            return await iamContext.ServiceHierarchies
                .Include(s => s.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Sno == serialNo);
        }

        public async Task Update(ServiceHierarchy entity)
        {
            iamContext.ServiceHierarchies.Update(entity);
            await iamContext.SaveChangesAsync();
        }

        public async Task<ServiceHierarchy> Add(ServiceHierarchy entity)
        {
            entity.Tenant = iamContext.Tenants.Single(t => t.TenantId == entity.TenantId);
            var added = iamContext.ServiceHierarchies.Add(entity);
            await iamContext.SaveChangesAsync();

            return added.Entity;
        }

        public async Task Delete(long serialNo)
        {
            await iamContext.ServiceHierarchies.Where(s => s.Sno == serialNo).ExecuteDeleteAsync();
        }
    }
}
