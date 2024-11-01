using Billing.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace BillingApi.Integration.Test.Fixtures
{
    public class TransactionalIamDatabaseFixture
    {
        private readonly string _localDbConnectionstring = "";

        public TransactionalIamDatabaseFixture()
        {
            var builder = new ConfigurationBuilder().AddUserSecrets<TransactionalIamDatabaseFixture>();
            var config = builder.Build();
            _localDbConnectionstring = config.GetSection("localDbConnectionstring").Value ?? "";

            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            CleanUp();
        }

        public void CleanUp()
        {
            using var context = CreateContext();

            //todo: 데이터를 정리하고 초기화 한다
            context.Tenants.RemoveRange(context.Tenants);
            context.ServiceHierarchies.RemoveRange(context.ServiceHierarchies);
            context.SaveChanges();

            context.Database.ExecuteSqlRaw("INSERT INTO IAM.dbo.Tenant (RealmName, OwnerId, TUId, IsActive, StartDate, EndDate, Remark, SavedAt, SaverId) VALUES(N'IAM', 1, N'972EAA63-6E7F-4F4F-8D78-8A1471E49CDB', 1, '2024-08-26 00:00:00.000', '2999-12-31 00:00:00.000', NULL, '2024-08-26 18:19:33.627', N'fe7ed185-ee53-4b59-a6b7-ca3b17b36f82');");
            var tenant = context.Tenants.First();

            context.ServiceHierarchies.Add(new ServiceHierarchy { TenantId = tenant.TenantId, ParentAccId = 0, AccountId = 1, IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(20), SavedAt = DateTime.UtcNow });

            context.SaveChanges();
        }

        public IAMContext CreateContext()
        {
            return new IAMContext(
                new DbContextOptionsBuilder<IAMContext>()
                    .UseSqlServer(_localDbConnectionstring)
                    .Options);
        }
    }
}
