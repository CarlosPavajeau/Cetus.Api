using Serilog;
using Serilog.Events;

namespace Cetus.Api.Configuration;

public static class Logging
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel
            .Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel
            .Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .Enrich.WithProperty("Application", "Cetus.Api")
            .WriteTo.Console()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateBootstrapLogger();

        builder.Services.AddSerilog();
        builder.Logging.AddSerilog();
    }
}
