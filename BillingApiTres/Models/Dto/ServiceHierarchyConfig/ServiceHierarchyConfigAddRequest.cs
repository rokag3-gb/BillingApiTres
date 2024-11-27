using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models;
using BillingApiTres.Models.Validations;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(ServiceHierarchyConfig), ReverseMap = true)]
    public record ServiceHierarchyConfigAddRequest
    {
        [SourceMember(nameof(ServiceHierarchyConfig.ConfigCode))]
        [ServiceHierarchyConfigCodeValidation]
        public string Code { get; set; }

        [SourceMember(nameof(ServiceHierarchyConfig.ConfigValue))]
        public double Value { get; set; }
    }
}
