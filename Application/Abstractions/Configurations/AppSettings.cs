namespace Application.Abstractions.Configurations;

public sealed class AppSettings
{
    public const string ConfigurationSection = "App";

    public string PublicUrl { get; set; }
    public string CdnUrl { get; set; }
}
