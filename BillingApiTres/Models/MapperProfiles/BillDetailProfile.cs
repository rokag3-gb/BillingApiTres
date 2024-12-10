using AutoMapper;
using Billing.Data.Models.Bill;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class BillDetailProfile : Profile
    {
        public BillDetailProfile() 
        {
            CreateMap<NcpDetail, BillDetailResponse>();
        }
    }
}
