using Cetus.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Api.Configuration;

public static class Database
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextPool<CetusDbContext>(options =>
        {
            options.UseNpgsql(
                    builder.Configuration.GetConnectionString("CetusContext"))
                .UseSnakeCaseNamingConvention();
        });
    }
}
