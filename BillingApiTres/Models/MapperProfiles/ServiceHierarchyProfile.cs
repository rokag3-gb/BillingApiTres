using AutoMapper;
using Billing.Data.Models;
using Billing.Data.Models.Sale;
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
                        .ForMember(dest => dest.ContractorKey, opt => opt.MapFrom(MapContractorKey))
                        .ForMember(dest => dest.ContractorName, opt => opt.MapFrom<string?>(MapContractorName))
                        .ForMember(dest => dest.ContracteeId, opt => opt.MapFrom(src => src.AccountId))
                        .ForMember(dest => dest.ContracteeKey, opt => opt.MapFrom(MapContracteeKey))
                        .ForMember(dest => dest.ContracteeName, opt => opt.MapFrom<string>(MapContracteeName))
                        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                        .ForMember(dest => dest.ContractDate, opt => opt.MapFrom(src => src.StartDate))
                        .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.EndDate))
                        .ForMember(dest => dest.Configs, opt => opt.MapFrom(src => src.ServiceHierarchyConfigs))
                        .ForMember(dest => dest.AccountLinkCount, opt => opt.MapFrom(MapAccountLinkCount))
                        .ForMember(dest => dest.AccountUserCount, opt => opt.MapFrom(MapAccountUserCount));
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

        private string MapContractorKey(ServiceHierarchy s, ServiceHierarchyResponse d, string m, ResolutionContext c)
        {
            object? accountKeyObj = default;

            if (c.TryGetItems(out var items) == false)
                return "";
            if (items.TryGetValue("accountKeys", out accountKeyObj) == false)
                return "";
            if ((accountKeyObj is List<AccountKey>) == false)
                return "";

            var accountKeys = accountKeyObj as List<AccountKey>;
#if DEBUG
            return accountKeys?.Where(ak => ak?.AccountId == s.ParentAccId)?.FirstOrDefault()?.AccountKey1 + $"({s.ParentAccId})" ?? "";
#else
            return accountKeys?.Where(ak => ak?.AccountId == s.ParentAccId)?.FirstOrDefault()?.AccountKey1 ?? "";
#endif
        }

        private string MapContracteeKey(ServiceHierarchy s, ServiceHierarchyResponse d, string m, ResolutionContext c)
        {
            object? accountKeyObj = default;

            if (c.TryGetItems(out var items) == false)
                return "";
            if (items.TryGetValue("accountKeys", out accountKeyObj) == false)
                return "";
            if ((accountKeyObj is List<AccountKey>) == false)
                return "";

            var accountKeys = accountKeyObj as List<AccountKey>;
#if DEBUG
            return accountKeys?.Where(ak => ak?.AccountId == s.AccountId)?.FirstOrDefault()?.AccountKey1 + $"({s.AccountId})" ?? "";
#else
            return accountKeys?.Where(ak => ak?.AccountId == s.AccountId)?.FirstOrDefault()?.AccountKey1 ?? "";
#endif
        }

        private int MapAccountUserCount(ServiceHierarchy s, ServiceHierarchyResponse d, int m, ResolutionContext c)
        {
            object? accountUserObj = default;

            if (c.TryGetItems(out var items) == false)
                return 0;
            if (items.TryGetValue("accountUser", out accountUserObj) == false)
                return 0;
            if ((accountUserObj is List<AccountUser>) == false)
                return 0;

            var accountUser = accountUserObj as List<AccountUser>;
            return accountUser?.Where(au => au?.AccountId == s.AccountId)?.Count() ?? 0;
        }

        private int MapAccountLinkCount(ServiceHierarchy s, ServiceHierarchyResponse d, int m, ResolutionContext c)
        {
            object? accountLinkObj = default;

            if (c.TryGetItems(out var items) == false)
                return 0;
            if (items.TryGetValue("accountLink", out accountLinkObj) == false)
                return 0;
            if ((accountLinkObj is List<AccountLink>) == false)
                return 0;

            var accountLink = accountLinkObj as List<AccountLink>;
            return accountLink?.Where(al => al.AccountId == s.AccountId).Count() ?? 0;
        }
    }
}
