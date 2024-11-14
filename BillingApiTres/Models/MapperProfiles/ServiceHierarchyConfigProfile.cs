using AutoMapper;
using BillingApiTres.Models.Dto;

namespace BillingApiTres.Models.MapperProfiles
{
    public class ServiceHierarchyConfigProfile : Profile
    {
        public ServiceHierarchyConfigProfile()
        {
            CreateMap<ServiceHierarchyConfigProfile, ServiceHierarchyConfigResponse>();
        }
    }
}
