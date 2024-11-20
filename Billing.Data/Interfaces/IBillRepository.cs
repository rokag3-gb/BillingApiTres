using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IBillRepository
    {
        List<Bill> GetRange(DateTime from, DateTime to, List<long> accountIds, int? offset, int? limit);
    }
}
