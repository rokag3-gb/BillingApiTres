using BillingApiTres.Models.Dto;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Runtime.Serialization;

namespace BillingApiTres.Helper
{
    public class EnumSchemaSwaggerFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();

                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name =>
                    {
                        schema.Enum.Add(new OpenApiString(
                            context.Type.GetField(name)?
                            .GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name));
                    });
            }
        }
    }
}
