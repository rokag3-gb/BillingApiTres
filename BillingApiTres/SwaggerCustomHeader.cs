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
                    Description = "요청지의 현지 시간대를 나타내는 헤더",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Default = new OpenApiString("Asia/Seoul")
                    }
                });

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "X-Account",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "요청 가능한 AccountId의 범위를 csv 형태로 정의한 헤더",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Default = new OpenApiString("1,2")
                    }
                });
        }
    }
}
