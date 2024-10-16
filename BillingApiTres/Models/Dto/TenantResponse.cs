using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(Tenant))]
    public record TenantResponse
    {
        [SourceMember(nameof(Tenant.TenantId))]
        public string? TenantId { get; set; } = null;
        [SourceMember(nameof(Tenant.RealmName))]
        public string Realm { get; set; } = string.Empty;
        [SourceMember(nameof(Tenant.OwnerId))]
        public int OwnerAccountId { get; set; } = -1;
        [SourceMember(nameof(Tenant.Tuid))]
        public Guid Tuid { get; set; } = Guid.Empty;
        [SourceMember(nameof(Tenant.Sno))]
        public int SerialNumber { get; set; } = -1;
        [SourceMember(nameof(Tenant.IsActive))]
        public bool IsActive { get; set; } = false;
        [SourceMember(nameof(Tenant.StartDate))]
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        [SourceMember(nameof(Tenant.EndDate))]
        public DateTime EndDate { get; set; } = DateTime.MinValue;
        [SourceMember(nameof(Tenant.Remark))]
        public string? Remark { get; set; } = null;
    }
}
