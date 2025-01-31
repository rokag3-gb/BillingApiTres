using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IBillRepository
    {
        List<Bill> Create(IEnumerable<Bill> bills);
        Task Delete(IEnumerable<long> billIds);
        Task Delete(IEnumerable<Bill> bills);
        List<Bill> GetLatestPublishedBill(IEnumerable<(DateTime BillDate, long BuyerAccountId, long? ConsumptionAccountId)> conditions);
        List<Bill> GetRange(DateTime? from, DateTime? to, List<long>? accountIds, List<long>? billIds, int? offset, int? limit);
        List<Bill> GetRangeWithRelations(DateTime? from, DateTime? to, List<long>? accountIds, List<long>? billIds, int? offset, int? limit, bool isNoTracking = false, bool includeDelete = false);
        int UpdateStatus(string statusCode, IEnumerable<long> billIds);
    }
}
