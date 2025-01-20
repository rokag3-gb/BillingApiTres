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
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
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
                .MinimumLevel.Debug()
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.WithProperty("AppNames", "Billing3")
                .Enrich.FromLogContext()
                .WriteTo.Console(formatter: new JsonFormatter())
                .WriteTo.Debug(new JsonFormatter());
            });
            return builder;
        }

        public static WebApplication UseSerilogFeatures(this WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. Request-Body: {RequestBody}";

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestBody", GetRequestBody(httpContext));
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

        private static string GetRequestBody(HttpContext httpContext)
        {
            if (httpContext.Request.Method == "GET" ||
                httpContext.Request.ContentLength == null ||
                httpContext.Request.ContentLength == 0)
            {
                return string.Empty;
            }

            try
            {
                httpContext.Request.EnableBuffering();
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                var body = reader.ReadToEndAsync().Result;
                httpContext.Request.Body.Position = 0;
                return body;
            }
            catch
            {
                return "[Error reading request body]";
            }
        }
    }
}
