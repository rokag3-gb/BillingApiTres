using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Iam;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(ServiceHierarchyConfig))]
    public record ServiceHierarchyConfigResponse
    {
        [SourceMember(nameof(ServiceHierarchyConfig.ConfigId))]
        public long ConfigId { get; set; }
        [SourceMember(nameof(ServiceHierarchyConfig.Sno))]
        public long Sno { get; set; }
        [SourceMember(nameof(ServiceHierarchyConfig.ConfigCode))]
        public string Code { get; set; }
        [SourceMember(nameof(ServiceHierarchyConfig.ConfigValue))]
        public double Value { get; set; }
    }
}
