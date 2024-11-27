using Billing.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingApi.Integration.Test.Fixtures
{
    /// <summary>
    /// 데이터 변경 사항이 적용되지 않는 동작에 사용되는 dbconext 테스트 대역을 생성합니다.
    /// </summary>
    [Obsolete("TransactionalIamDatabaseFixture와 같이 사용될 경우 context.Database.EnsureDeleted(); 코드에서 sql 권한 이슈 발생하여 사용 보류")]
    public class IamDatabaseTestFixture
    {
        private readonly string _localDbConnectionstring = "";
        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public IamDatabaseTestFixture()
        {
            lock (_lock)
            {
                if (_databaseInitialized)
                    return;

                var builder = new ConfigurationBuilder().AddUserSecrets<IamDatabaseTestFixture>();
                var config = builder.Build();
                _localDbConnectionstring = config.GetSection("localDbConnectionstring").Value ?? "";

                using var context = CreateConext();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //todo: context에 테스트 데이터 추가
                //context.Tenants.Add(new Tenant { RealmName = "IAM", Tuid = new Guid("972EAA63-6E7F-4F4F-8D78-8A1471E49CDB"), IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), SavedAt = DateTime.UtcNow });
                context.Database.ExecuteSqlRaw("INSERT INTO IAM.dbo.Tenant (RealmName, OwnerId, TUId, IsActive, StartDate, EndDate, Remark, SavedAt, SaverId) VALUES(N'IAM', 1, N'972EAA63-6E7F-4F4F-8D78-8A1471E49CDB', 1, '2024-08-26 00:00:00.000', '2999-12-31 00:00:00.000', NULL, '2024-08-26 18:19:33.627', N'fe7ed185-ee53-4b59-a6b7-ca3b17b36f82');");
                context.ServiceHierarchies.Add(new ServiceHierarchy { TenantId = "iam-cdb01", ParentAccId = 0, AccountId = 1, IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(20), SavedAt = DateTime.UtcNow });
                context.SaveChanges();

                _databaseInitialized = true;
            }
        }

        public IAMContext CreateConext()
        {
            return new IAMContext(
                new DbContextOptionsBuilder<IAMContext>()
                    .UseSqlServer(_localDbConnectionstring)
                    .Options);
        }
    }
}
