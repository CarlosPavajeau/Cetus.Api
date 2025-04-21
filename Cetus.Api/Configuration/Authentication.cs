using Clerk.Net.AspNetCore.Security;

namespace Cetus.Api.Configuration;

public static class Authentication
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(ClerkAuthenticationDefaults.AuthenticationScheme)
            .AddClerkAuthentication(options =>
            {
                options.Authority = builder.Configuration["Clerk:Authority"]!;
                options.AuthorizedParty = builder.Configuration["Clerk:AuthorizedParty"]!;
            });

        return builder;
    }
}
