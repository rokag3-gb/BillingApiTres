using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(ServiceHierarchy))]
    public record ServiceHierarchyResponse
    {
        [SourceMember(nameof(ServiceHierarchy.Sno))]
        public int SerialNo { get; set; }
        [SourceMember(nameof(ServiceHierarchy.TenantId))]
        public string TenantId { get; set; }
        //[SourceMember(nameof(ServiceHierarchy.Tenant.RealmName))]
        public string RealmName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.ParentAccId))]
        public int ContractorId { get; set; }
        public string? ContractorName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.AccountId))]
        public int ContracteeId { get; set; }
        public string ContracteeName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.IsActive))]
        public bool IsActive { get; set; }
        [SourceMember(nameof(ServiceHierarchy.StartDate))]
        public DateTime ContractDate { get; set; }
        [SourceMember(nameof(ServiceHierarchy.EndDate))]
        public DateTime ExpireDate { get; set; }

    }
}
