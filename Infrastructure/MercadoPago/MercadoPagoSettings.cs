using System.ComponentModel.DataAnnotations;

namespace Infrastructure.MercadoPago;

public sealed class MercadoPagoSettings
{
    public const string ConfigurationSection = "MercadoPago";

    [Required] public string AccessToken { get; set; }
    [Required] public string ClientSecret { get; set; }
    [Required] public string ClientId { get; set; }
    [Required] public string RedirectUri { get; set; }
    [Required] public string BackUrl { get; set; }
}
