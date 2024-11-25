using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;
using BillingApiTres.Models.Validations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(ServiceHierarchy), ReverseMap = true)]
    public record ServiceHierarchyAddRequest
    {
        [Required]
        [SourceMember(nameof(ServiceHierarchy.TenantId))]
        public string TenantId { get; set; }

        [Required]
        [SourceMember(nameof(ServiceHierarchy.ParentAccId))]
        [JsonIgnore]
        public long ContractorId { get; set; }

        [JsonPropertyName("contractorId")]
        public string ContractorKey { get; set; }

        [Required]
        [SourceMember(nameof(ServiceHierarchy.AccountId))]
        [JsonIgnore]
        public long ContracteeId { get; set; }

        [JsonPropertyName("contracteeId")]
        public string ContracteeKey { get; set; }

        [Required]
        [SourceMember(nameof(ServiceHierarchy.IsActive))]
        public bool IsActive { get; set; }

        [Required]
        //[Iso8601UtcValidation]
        [SourceMember(nameof(ServiceHierarchy.StartDate))]
        public DateTime ContractDate { get; set; }

        [Required]
        //[Iso8601UtcValidation]
        [SourceMember(nameof(ServiceHierarchy.EndDate))]
        public DateTime ExpireDate { get; set; }

        [SourceMember(nameof(ServiceHierarchy.ServiceHierarchyConfigs))]
        public ICollection<ServiceHierarchyConfigAddRequest>? Configs { get; set; }
    }
}
