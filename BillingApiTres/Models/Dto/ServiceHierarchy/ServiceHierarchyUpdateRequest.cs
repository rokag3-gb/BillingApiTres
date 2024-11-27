using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BillingApiTres.Models.Dto
{
    public record ServiceHierarchyUpdateRequest : IValidatableObject
    {
        public bool? IsActive { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public ICollection<ServiceHierarchyConfigUpdateRequest>? Configs { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsActive == null && ContractDate == null && ExpireDate == null)
                yield return new ValidationResult("갱신 값이 없습니다.");
        }
    }
}
