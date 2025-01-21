using BillingApiTres.Constants;
using BillingApiTres.Converters;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace BillingApiTres
{
    public static class ObservabilityBuilder
    {
        public static WebApplicationBuilder ConfigureObservabilities(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestBodyLogLimit = int.MaxValue;
                logging.ResponseBodyLogLimit = int.MaxValue;
                logging.CombineLogs = true;
            });

            builder.Host.UseSerilog((context, serviceProvider, configuration) =>
            {
                configuration
                //.MinimumLevel.Debug()
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.WithProperty("AppNames", "Billing3")
                .Enrich.FromLogContext();
            });
            return builder;
        }

        public static WebApplication UseSerilogFeatures(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                "User: {User} " +
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. " +
                "Request-Body: {@RequestBody} " +
                "Reponse-Body: {@ResponseBody}";

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestBody", httpContext.Items[HttpConstants.RequestBody] ?? string.Empty);
                    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? string.Empty);
                    diagnosticContext.Set("ContentType", httpContext.Request.ContentType ?? string.Empty);
                    diagnosticContext.Set("Host", httpContext.Request.Host.Value);
                    diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
                    diagnosticContext.Set("Scheme", httpContext.Request.Scheme);

                    diagnosticContext.Set(
                        "User",
                        JwtConverter.ExtractJwtToken(httpContext.Request)?.Claims
                                    .FirstOrDefault(c => c.Type == "email")?.Value ?? "not set user email");
                };

                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null)
                        return LogEventLevel.Error;
                    if (httpContext.Response.StatusCode >= 500)
                        return LogEventLevel.Error;
                    if (httpContext.Response.StatusCode >= 400)
                        return LogEventLevel.Warning;

                    return LogEventLevel.Information;
                };
            });
            return app;
        }
    }
}
