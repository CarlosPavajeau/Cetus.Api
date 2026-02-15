using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Wompi;

public sealed class WompiSettings
{
    public const string ConfigurationSection = "Wompi";

    [Required] public string EventSecret { get; set; }
    [Required] public string BaseUrl { get; set; }
}
