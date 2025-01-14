using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IBillRepository
    {
        List<Bill> GetRange(DateTime? from, DateTime? to, List<long>? accountIds, List<long>? billIds, int? offset, int? limit);
        List<Bill> GetRangeWithRelations(DateTime from, DateTime to, List<long> accountIds, int? offset, int? limit);
        int UpdateStatus(string statusCode, IEnumerable<long> billIds);
    }
}
