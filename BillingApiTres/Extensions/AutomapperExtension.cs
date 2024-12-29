using AutoMapper;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.MapperProfiles;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingApiTres.Extensions
{
    public static class AutomapperExtension
    {
        public static IServiceCollection AddMapperBillingTypes(this IServiceCollection collection)
        {
            collection.AddAutoMapper(typeof(TenantResponse));
            collection.AddAutoMapper(config =>
            {
                config.AddProfile(typeof(ServiceHierarchyProfile));
                config.AddProfile(typeof(BillProfile));
            });

            return collection;
        }

        /// <summary>
        /// Key 특성을 설정하거나 ForeignKey 특성에 선언된 속성을 매핑에서 제외합니다
        /// </summary>
        public static IMappingExpression<TSource, TDestination> IgnoreKeyProperties<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression)
        {
            var primaryKeyProperties = typeof(TDestination)
                .GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .ToList();

            var foreignKeyProperties = new HashSet<string>();
            foreach (var property in typeof(TDestination).GetProperties())
            {
                var foreignKeyAttr = property.GetCustomAttributes(typeof(ForeignKeyAttribute), false)
                                             .FirstOrDefault();
                if (foreignKeyAttr is ForeignKeyAttribute attr)
                {
                    // ForeignKey 특성의 인자값이 실제 외래 키 속성 이름임
                    foreignKeyProperties.Add(attr.Name);
                }
            }

            primaryKeyProperties.ForEach(p => expression.ForMember(p.Name, options => options.Ignore()));
            foreach (var prop in foreignKeyProperties)
            {
                expression.ForMember(prop, options => options.Ignore());
            }

            return expression;
        }
    }
}
