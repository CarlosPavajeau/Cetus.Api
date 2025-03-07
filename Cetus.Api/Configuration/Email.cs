using Resend;

namespace Cetus.Api.Configuration;

public static class Email
{
    public static void ConfigureEmail(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddHttpClient<ResendClient>();

        builder.Services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = builder.Configuration["Resend:ApiToken"]!;
        });

        builder.Services.AddTransient<IResend, ResendClient>();
    }
}
