namespace Domain.Stores;

public class Store
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }

    public bool IsActive { get; set; }

    public string? LogoUrl { get; set; }

    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? MercadoPagoAccessToken { get; set; }
    public string? MercadoPagoRefreshToken { get; set; }
    public DateTime? MercadoPagoExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsConnectedToMercadoPago => !string.IsNullOrEmpty(MercadoPagoAccessToken) &&
                                            !string.IsNullOrEmpty(MercadoPagoRefreshToken);

    public bool IsMercadoPagoTokenExpired =>
        MercadoPagoExpiresAt.HasValue && MercadoPagoExpiresAt.Value <= DateTime.Today;
}
