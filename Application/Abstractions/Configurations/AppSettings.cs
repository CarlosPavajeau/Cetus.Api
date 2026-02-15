using System.ComponentModel.DataAnnotations;

namespace Application.Abstractions.Configurations;

public sealed class AppSettings
{
    public const string ConfigurationSection = "App";

    [Required] public string PublicUrl { get; set; }
    [Required] public string CdnUrl { get; set; }
}
