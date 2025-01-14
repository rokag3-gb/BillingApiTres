using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IBillItemRepository
    {
        List<BillItem> GetList(long billId, int? offset, int? limit);
        IEnumerable<BillItem> GetListVenderDetail(long billId, int? offset, int? limit);
    }
}
