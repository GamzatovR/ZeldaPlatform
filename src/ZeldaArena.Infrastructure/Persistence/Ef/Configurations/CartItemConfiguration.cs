using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class CartItemConfiguration : EntityConfiguration<CartItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.Ignore(item => item.LineTotal);

        builder.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();

        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}