using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Configurations;

public sealed class JwtSettings
{
    public const string ConfigurationSection = "Jwt";

    [Required] public string Issuer { get; set; }
    [Required] public string Audience { get; set; }
    [Required] public string Authority { get; set; }
    public int ExpirationInMinutes { get; set; }
}
