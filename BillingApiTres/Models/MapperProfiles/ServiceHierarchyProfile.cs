using AutoMapper;
using Billing.Data.Models;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class ServiceHierarchyProfile : Profile
    {
        public ServiceHierarchyProfile()
        {
            CreateMap<ServiceHierarchy, ServiceHierarchyResponse>()
                        .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                        .ForMember(dest => dest.SerialNo, opt => opt.MapFrom(src => src.Sno))
                        .ForMember(dest => dest.RealmName, opt => opt.MapFrom(src => src.Tenant.RealmName))
                        .ForMember(dest => dest.ContractorId, opt => opt.MapFrom(src => src.ParentAccId))
                        .ForMember<string>(dest => dest.ContractorName, opt => opt.MapFrom<string?>(MapContractorName))
                        .ForMember(dest => dest.ContracteeId, opt => opt.MapFrom(src => src.AccountId))
                        .ForMember(dest => dest.ContracteeName, opt => opt.MapFrom<string>(MapContracteeName))
                        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                        .ForMember(dest => dest.ContractDate, opt => opt.MapFrom(src => src.StartDate))
                        .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.EndDate))
                        .ForMember(dest => dest.Configs, opt => opt.MapFrom(src => src.ServiceHierarchyConfigs));
        }

        private string MapContractorName(ServiceHierarchy s, ServiceHierarchyResponse d, string m, ResolutionContext c)
        {
            object? accountsObj = default;
            if (c.TryGetItems(out var items) == false)
                return "";
            if (items.TryGetValue("accounts", out accountsObj) == false)
                return "";
            if ((accountsObj is List<SalesAccount>) == false)
                return "";

            var accounts = accountsObj as List<SalesAccount>;
            return accounts?.Where(a => a.AccountId == s.ParentAccId).FirstOrDefault()?.AccountName ?? "";
        }

        private string MapContracteeName(ServiceHierarchy s, ServiceHierarchyResponse d, string m, ResolutionContext c)
        {
            object? accountsObj = default;
            if (c.TryGetItems(out var items) == false)
                return "";
            if (items.TryGetValue("accounts", out accountsObj) == false)
                return "";
            if ((accountsObj is List<SalesAccount>) == false)
                return "";

            var accounts = accountsObj as List<SalesAccount>;
            return accounts?.Where(a => a?.AccountId == s.AccountId).FirstOrDefault()?.AccountName ?? "";
        }
    }
}
