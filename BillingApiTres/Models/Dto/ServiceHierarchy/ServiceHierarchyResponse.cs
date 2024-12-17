using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Iam;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(ServiceHierarchy))]
    public record ServiceHierarchyResponse
    {
        [SourceMember(nameof(ServiceHierarchy.Sno))]
        public long SerialNo { get; set; }
        [SourceMember(nameof(ServiceHierarchy.TenantId))]
        public string TenantId { get; set; }
        //[SourceMember(nameof(ServiceHierarchy.Tenant.RealmName))]
        public string RealmName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.ParentAccId))]
        public long ContractorId { get; set; }
        public string? ContractorName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.AccountId))]
        public long ContracteeId { get; set; }
        public string ContracteeName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.IsActive))]
        public bool IsActive { get; set; }
        [SourceMember(nameof(ServiceHierarchy.StartDate))]
        public DateTime ContractDate { get; set; }
        [SourceMember(nameof(ServiceHierarchy.EndDate))]
        public DateTime ExpireDate { get; set; }
        [SourceMember(nameof(ServiceHierarchy.ServiceHierarchyConfigs))]
        public ICollection<ServiceHierarchyConfigResponse>? Configs { get; set; }
        [SourceMember(nameof(ServiceHierarchy.TypeCode))]
        public string? Type { get; set; }
        public string? TypeName { get; set; }
        public int AccountUserCount { get; set; }
        public int AccountLinkCount { get; set; }
    }
}
