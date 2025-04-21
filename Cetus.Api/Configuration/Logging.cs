using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Cetus.Api.Configuration;

public static class Logging
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);

            var applicationInsightsConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (string.IsNullOrEmpty(applicationInsightsConnectionString))
            {
                return;
            }

            configuration.WriteTo
                .ApplicationInsights(applicationInsightsConnectionString, new TraceTelemetryConverter())
                .WriteTo
                .OpenTelemetry();
        });

        return builder;
    }
}
