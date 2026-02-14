namespace Infrastructure.Wompi;

public sealed class WompiSettings
{
    public const string ConfigurationSection = "Wompi";

    public string EventSecret { get; set; }
    public string BaseUrl { get; set; }
}
