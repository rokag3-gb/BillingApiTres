using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        [SourceMember(nameof(ServiceHierarchy.ParentAccId))]
        public long ContractorId { get; set; }
        [JsonPropertyName("contractorId")]
        public string ContractorKey { get; set; }
        public string? ContractorName { get; set; }
        [JsonIgnore]
        [SourceMember(nameof(ServiceHierarchy.AccountId))]
        public long ContracteeId { get; set; }
        [JsonPropertyName("contracteeId")]
        public string ContracteeKey { get; set; }
        public string ContracteeName { get; set; }
        [SourceMember(nameof(ServiceHierarchy.IsActive))]
        public bool IsActive { get; set; }
        [SourceMember(nameof(ServiceHierarchy.StartDate))]
        public DateTime ContractDate { get; set; }
        [SourceMember(nameof(ServiceHierarchy.EndDate))]
        public DateTime ExpireDate { get; set; }
        [SourceMember(nameof(ServiceHierarchy.ServiceHierarchyConfigs))]
        public ICollection<ServiceHierarchyConfigResponse>? Configs { get; set; }
        public int AccountUserCount { get; set; }
        public int AccountLinkCount { get; set; }
    }
}
