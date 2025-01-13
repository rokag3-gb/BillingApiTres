using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Microsoft.EntityFrameworkCore;

namespace Billing.EF.Repositories
{
    public class ProductRepository(BillContext billContext) : IProductRepository
    {
        public List<Product> GetList()
        {
            return billContext.Products.AsNoTracking().ToList();
        }
    }
}
