namespace Infrastructure.MercadoPago;

public sealed class MercadoPagoSettings
{
    public const string ConfigurationSection = "MercadoPago";

    public string AccessToken { get; set; }
    public string ClientSecret { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string BackUrl { get; set; }
}
