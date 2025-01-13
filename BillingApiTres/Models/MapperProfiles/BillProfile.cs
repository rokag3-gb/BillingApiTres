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
            CreateMap<Bill, Bill>().IgnoreKeyProperties()
                .ForMember(dest => dest.OriginalBillId, opt => opt.MapFrom(src => src.BillId))
                //.ForMember(dest => dest.BillDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SavedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<BillItem, BillItem>().IgnoreKeyProperties()
                .ForMember(dest => dest.SavedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.SaverId, opt => opt.MapFrom(src => src.Bill.SaverId));
        }
    }
}
