using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Billing.Data.Models.Bill;

namespace BillingApiTres.Models.Dto
{
    [AutoMap(typeof(BillingRole))]
    public record BillRoleResponse
    {
        [SourceMember(nameof(BillingRole.RoleId))]
        public string BillRoleId { get; set; }
        [SourceMember(nameof(BillingRole.Alias))]
        public string RoleName { get; set; }
    }
}
