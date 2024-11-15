using AutoMapper;
using Billing.Data.Models;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class ServiceHierarchyConfigProfile : Profile
    {
        public ServiceHierarchyConfigProfile()
        {
            CreateMap<ServiceHierarchyConfig, ServiceHierarchyConfigResponse>();
            CreateMap<ServiceHierarchyConfigAddRequest, ServiceHierarchyConfig>();
        }
    }
}
