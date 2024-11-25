using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BillingApiTres
{
    /// <summary>
    /// Swagger에 사용자 헤더를 추가합니다.
    /// </summary>
    public class SwaggerCustomHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters.Add(
                new OpenApiParameter               
                {
                    Name = "X-TimeZone",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Default = new OpenApiString("Asia/Seoul")
                    }
                });
        }
    }
}
