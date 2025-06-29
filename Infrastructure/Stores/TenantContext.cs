using Application.Abstractions.Data;

namespace Infrastructure.Stores;

public class TenantContext : ITenantContext
{
    public Guid Id { get; set; }
}
