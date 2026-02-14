namespace Infrastructure.Configurations;

public sealed class JwtSettings
{
    public const string ConfigurationSection = "Jwt";

    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Authority { get; set; }
    public int ExpirationInMinutes { get; set; }
}
