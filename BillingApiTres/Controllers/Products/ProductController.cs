using AutoMapper;
using Billing.Data.Interfaces;
using BillingApiTres.Models.Dto.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingApiTres.Controllers.Products
{
    [Route("[controller]")]
    [Authorize]
    public class ProductController(IProductRepository billRepository,
                                   IMapper mapper,
                                   ILogger<ProductController> logger) : ControllerBase
    {
        [HttpGet("/products")]
        public List<ProductListResponse> GetList()
        {
            return billRepository
                .GetList()
                .Select(mapper.Map<ProductListResponse>)
                .ToList();
        }
    }
}
