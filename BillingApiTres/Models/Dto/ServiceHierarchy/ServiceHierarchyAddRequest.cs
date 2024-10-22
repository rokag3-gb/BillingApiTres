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
        public long ContractorId { get; set; }

        [Required]
        [SourceMember(nameof(ServiceHierarchy.AccountId))]
        public long ContracteeId { get; set; }

        [Required]
        [SourceMember(nameof(ServiceHierarchy.IsActive))]
        public bool IsActive { get; set; }

        [Required]
        [Iso8601UtcValidation]
        [SourceMember(nameof(ServiceHierarchy.StartDate))]
        public DateTime ContractDate { get; set; }

        [Required]
        [Iso8601UtcValidation]
        [SourceMember(nameof(ServiceHierarchy.EndDate))]
        public DateTime ExpireDate { get; set; }
    }
}
