namespace Cetus.Api.Configuration;

public static class Cors
{
    public const string AllowAll = "AllowAll";

    public static void ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(AllowAll, policy =>
            {
                var allowedOrigin = builder.Configuration["AllowedOrigin"]!;

                policy
                    .WithOrigins(allowedOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }
}
