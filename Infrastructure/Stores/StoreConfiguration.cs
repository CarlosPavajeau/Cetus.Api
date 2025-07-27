using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Stores;

internal sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.Ignore(s => s.IsConnectedToMercadoPago);
        builder.Ignore(s => s.IsMercadoPagoTokenExpired);
    }
}
