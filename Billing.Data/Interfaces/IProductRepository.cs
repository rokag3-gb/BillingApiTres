using Billing.Data.Models.Bill;

namespace Billing.Data.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetList();
    }
}
