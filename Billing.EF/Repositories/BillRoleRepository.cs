using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;

namespace Billing.EF.Repositories
{
    public class BillRoleRepository(BillContext billContext) : IBillRoleRepository
    {
        public List<BillingRole> GetAll()
        {
            return billContext.BillingRoles.ToList();
        }
    }
}
