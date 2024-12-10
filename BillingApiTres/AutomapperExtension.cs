using BillingApiTres.Models.Dto;
using BillingApiTres.Models.MapperProfiles;

namespace BillingApiTres
{
    public static class AutomapperExtension
    {
        public static IServiceCollection AddMapperBillingTypes(this IServiceCollection collection)
        {
            collection.AddAutoMapper(typeof(TenantResponse));
            collection.AddAutoMapper(config =>
            {
                config.AddProfile(typeof(ServiceHierarchyProfile));
                config.AddProfile(typeof(BillDetailProfile));
            });
            return collection;
        }
    }
}
