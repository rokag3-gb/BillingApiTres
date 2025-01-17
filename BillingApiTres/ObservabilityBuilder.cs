using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

            builder.Host.UseSerilog((context, serviceProvider, configuration) =>
            {
                configuration
                .MinimumLevel.Debug()
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.WithProperty("AppNames", "Billing3")
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug();
            });
            return builder;
        }
    }
}
