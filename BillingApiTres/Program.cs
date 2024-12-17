using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Billing.Data.Models.Iam;
using Billing.Data.Models.Sale;
using Billing.EF.Repositories;
using BillingApiTres.Converters;
using BillingApiTres.Helper;
using BillingApiTres.Models.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;


[assembly: ApiController]
namespace BillingApiTres
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region regist repositories
            builder.Services.AddScoped<ITenantRepository, TenantRepository>();
            builder.Services.AddScoped<IServiceHierarchyRepository, ServiceHierarchyRepository>();
            builder.Services.AddScoped<IAccountKeyRepository, AccountKeyRepository>();
            builder.Services.AddScoped<IBillRepository, BillRepository>();
            builder.Services.AddScoped<IBillDetailRepository, BillDetailRepository>();
            builder.Services.AddScoped<IBillItemRepository, BillItemRepository>();
            builder.Services.AddScoped<INcpRepository, NcpRepository>();
            builder.Services.AddScoped<IBillRoleRepository, BillRoleRepository>();
            #endregion

            #region regist Http Client
            builder.Services.AddHttpClient<AcmeGwClient>(c => c.BaseAddress = new Uri(builder.Configuration["gateway_url"]!));
            builder.Services.AddHttpClient<ServiceAccountTokenClient>(c => c.BaseAddress = new Uri(builder.Configuration["gateway_url"]!));
            #endregion

            builder.Services.AddTransient<CurrencyConverter>();
            builder.Services.AddTransient<ITimeZoneConverter, IanaDatetimeConverter>();
            
            builder.Services.AddSingleton<ServiceAccountTokenStorage>();
            builder.Services.Configure<ServiceAccountRequestBody>(
                builder.Configuration.GetSection("ServiceAccount"));

            builder.Services.AddMapperBillingTypes();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = false,
                        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
                        SignatureValidator = (t, p) => new JsonWebToken(t)
                    };
                });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing3", Version = "v1" });
                options.SchemaFilter<EnumSchemaSwaggerFilter>();
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BillingApiTres.xml"));

                var securityScheme = new OpenApiSecurityScheme
                {
                    Description = @"Header 의 Authorization에 들어갈 JWT Bearer 인가 토큰. (예시: `eyJ...In0.eyJ...CJ9.ZLo...IDQ`)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                };
                var securityRequirement = new OpenApiSecurityRequirement {
                    {  securityScheme, new string[] {} } };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                //#if DEBUG
                options.OperationFilter<SwaggerCustomHeader>();
                //#endif
            });

            builder.Services.AddDbContext<IAMContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["IamDbConnection"]);
                options.EnableSensitiveDataLogging();
            });
            builder.Services.AddDbContext<SaleContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["SaleDbConnection"]);
                options.EnableSensitiveDataLogging();
            });
            builder.Services.AddDbContext<BillContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["BillDbConnection"]);
                options.EnableSensitiveDataLogging();
            });


            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();
            app.MapControllers();

            app.UseTimezoneHeaderChecker();
            app.UseAccountHeaderChecker();

            app.Run();
        }
    }
}
