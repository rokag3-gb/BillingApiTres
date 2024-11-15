using BillingApiTres.Models.Validations;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace BillingApiTres.Models.Dto
{
    public record ServiceHierarchyConfigUpdateRequest
    {
        public long? ConfigId { get; set; }
        [ServiceHierarchyConfigCodeValidation]
        public string Code { get; set; }
        [Range(0, double.MaxValue)]
        public double Value { get; set; }
    }
}
