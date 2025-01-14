using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IBillRoleRepository
    {
        List<BillingRole> GetAll();
    }
}
