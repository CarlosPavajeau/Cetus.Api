using Cetus.Infrastructure.Persistence.EntityFramework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cetus.Api.Test.Shared;

public class ApplicationTestCase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("test");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CetusDbContext>));

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            services.AddDbContextPool<CetusDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.UseInternalServiceProvider(serviceProvider);
            });
        });

        base.ConfigureWebHost(builder);
    }
}
