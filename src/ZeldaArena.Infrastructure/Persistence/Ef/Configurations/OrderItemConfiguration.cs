using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class OrderItemConfiguration : EntityConfiguration<OrderItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.Property(item => item.ProductNameSnapshot).IsRequired().HasMaxLength(200);

        builder.Ignore(item => item.LineTotal);

        // Restrict: снятый с продажи товар не должен уносить с собой позиции заказов.
        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.OrderId);
    }
}