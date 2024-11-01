using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingApi.Integration.Test.Fixtures
{
    [CollectionDefinition(nameof(TransactionalIamDatabaseFixture))]
    public class TransactionalIamTestCollection : ICollectionFixture<TransactionalIamDatabaseFixture>
    {
    }
}
