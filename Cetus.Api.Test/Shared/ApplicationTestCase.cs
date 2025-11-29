using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Infrastructure.Database;
using Infrastructure.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Resend;
using SharedKernel;

namespace Cetus.Api.Test.Shared;

public class ApplicationTestCase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("test");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
                options.UseInternalServiceProvider(serviceProvider);
                options.ConfigureWarnings(warnings => { warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning); });
            });

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Mock IResend
            var resendMock = new Mock<IResend>();
            resendMock.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResendResponse<Guid>(Guid.Empty, null));

            services.RemoveAll<IResend>();
            services.AddSingleton(resendMock.Object);

            // Mock IDateTimeProvider
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            var frozenCurrentTime = DateTime.UtcNow;
            dateTimeProviderMock.Setup(dp => dp.UtcNow).Returns(frozenCurrentTime);

            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton(dateTimeProviderMock.Object);

            var tenantId = Guid.NewGuid();
            services.RemoveAll<ITenantContext>();
            services.AddSingleton<ITenantContext>(new TenantContext {Id = tenantId});

            // Replace stock reservation with in-memory implementation for tests
            services.RemoveAll<IStockReservationService>();
            services.AddScoped<IStockReservationService, InMemoryStockReservationService>();
        });

        base.ConfigureWebHost(builder);
    }
}
