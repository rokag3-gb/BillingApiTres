using AutoMapper;
using Billing.Data.Models.Iam;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class ServiceHierarchyProfile : Profile
    {
        public ServiceHierarchyProfile()
        {
            CreateMap<ServiceHierarchy, ServiceHierarchyResponse>()
                .AfterMap<ServiceHierarchyResponseMappingAction>();
        }
    }

    public class ServiceHierarchyResponseMappingAction : IMappingAction<ServiceHierarchy, ServiceHierarchyResponse>
    {
        private readonly ITimeZoneConverter _timezoneConverter;
        public ServiceHierarchyResponseMappingAction(ITimeZoneConverter timeZoneConverter)
        {
            _timezoneConverter = timeZoneConverter;
        }

        public void Process(ServiceHierarchy source, ServiceHierarchyResponse destination, ResolutionContext context)
        {
            destination.TenantId = source.TenantId;
            destination.SerialNo = source.Sno;
            destination.RealmName = source.Tenant.RealmName;
            destination.ContractorId = source.ParentAccId;
            destination.ContractorName = MapAccountName(source.ParentAccId, context);
            destination.ContracteeId = source.AccountId;
            destination.ContracteeName = MapAccountName(source.AccountId, context);
            destination.IsActive = source.IsActive;
            destination.ContractDate = ConvertToLocalTime(source.StartDate, context);
            destination.ExpireDate = ConvertToLocalTime(source.EndDate, context);
            destination.Configs = context.Mapper.Map<ICollection<ServiceHierarchyConfigResponse>>(source.ServiceHierarchyConfigs);
            destination.AccountLinkCount = MapAccountLinkCount(source.AccountId, context);
            destination.AccountUserCount = MapAccountUserCount(source.AccountId, context);
        }

        private string MapAccountName(long accountId, ResolutionContext context)
        {
            object? accountsObj = default;
            if (context.TryGetItems(out var items) == false)
                return "";
            if (items.TryGetValue("accounts", out accountsObj) == false)
                return "";
            if ((accountsObj is List<SalesAccount>) == false)
                return "";

            var accounts = accountsObj as List<SalesAccount>;
            return accounts?.Where(a => a?.AccountId == accountId).FirstOrDefault()?.AccountName ?? "";
        }

        private DateTime ConvertToLocalTime(DateTime dateTime, ResolutionContext context)
        {
            object? tzObject = default;
            if (context.TryGetItems(out var items) == false)
                return DateTime.MinValue;
            if (items.TryGetValue("timezone", out  tzObject) == false)
                return DateTime.MinValue;
            
            var tz = tzObject.ToString();
            return _timezoneConverter.ConvertToLocal(dateTime, tz!);
        }

        private int MapAccountUserCount(long accountId, ResolutionContext c)
        {
            object? accountUserObj = default;

            if (c.TryGetItems(out var items) == false)
                return 0;
            if (items.TryGetValue("accountUser", out accountUserObj) == false)
                return 0;
            if ((accountUserObj is List<AccountUser>) == false)
                return 0;

            var accountUser = accountUserObj as List<AccountUser>;
            return accountUser?.Where(au => au?.AccountId == accountId)?.Count() ?? 0;
        }

        private int MapAccountLinkCount(long accountId, ResolutionContext c)
        {
            object? accountLinkObj = default;

            if (c.TryGetItems(out var items) == false)
                return 0;
            if (items.TryGetValue("accountLink", out accountLinkObj) == false)
                return 0;
            if ((accountLinkObj is List<AccountLink>) == false)
                return 0;

            var accountLink = accountLinkObj as List<AccountLink>;
            return accountLink?.Where(al => al.AccountId == accountId).Count() ?? 0;
        }
    }
}
