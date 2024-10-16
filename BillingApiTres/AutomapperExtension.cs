using BillingApiTres.Models.Dto;
using System.Runtime.CompilerServices;

namespace BillingApiTres
{
    public static class AutomapperExtension
    {
        public static IServiceCollection AddMapperBillingTypes(this IServiceCollection collection)
        {
            collection.AddAutoMapper(typeof(TenantResponse));
            return collection;
        }
    }
}
