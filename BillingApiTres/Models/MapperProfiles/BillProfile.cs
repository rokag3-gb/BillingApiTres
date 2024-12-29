using AutoMapper;
using Billing.Data.Models.Bill;
using BillingApiTres.Extensions;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class BillProfile : Profile
    {
        public BillProfile() 
        {
            CreateMap<NcpDetail, BillDetailResponse>();
            CreateMap<Bill, Bill>().IgnoreKeyProperties();
            CreateMap<BillItem, BillItem>().IgnoreKeyProperties();
            CreateMap<BillDetail, BillDetail>().IgnoreKeyProperties();
            CreateMap<Product, Product>().IgnoreKeyProperties();
        }
    }
}
