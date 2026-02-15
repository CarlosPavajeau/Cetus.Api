using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Email;

public sealed class ResendSettings
{
    public const string ConfigurationSection = "Resend";

    [Required] public string ApiToken { get; set; }
    [Required] public string From { get; set; }
}
