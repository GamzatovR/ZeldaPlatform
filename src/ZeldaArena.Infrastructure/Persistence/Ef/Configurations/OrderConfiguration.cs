using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class OrderConfiguration : EntityConfiguration<Order>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.Property(order => order.Number).IsRequired().HasMaxLength(30);
        builder.HasIndex(order => order.Number).IsUnique();

        builder.Property(order => order.Currency).IsRequired().HasMaxLength(3).IsFixedLength();

        builder.Ignore(order => order.TotalMoney);
        builder.Ignore(order => order.IsPaid);

        // Адрес доставки — объект-значение, разложенный в шесть столбцов заказа.
        // Хранится снапшотом: заказ не меняется вслед за профилем пользователя.
        builder.ComplexProperty(order => order.Address, address =>
        {
            address.Property(value => value.Recipient).HasColumnName("Recipient").HasMaxLength(150);
            address.Property(value => value.Phone).HasColumnName("Phone").HasMaxLength(30);
            address.Property(value => value.Country).HasColumnName("Country").HasMaxLength(60);
            address.Property(value => value.City).HasColumnName("City").HasMaxLength(120);
            address.Property(value => value.Street).HasColumnName("Street").HasMaxLength(250);
            address.Property(value => value.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
        });

        builder.UseXminAsConcurrencyToken();

        // Restrict: финансовая история не удаляется вместе с пользователем.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Индекс под фильтр истории заказов (docs/SPEC.md §10.2).
        builder.HasIndex(order => new { order.UserId, order.Status, order.PlacedAt });
        builder.HasIndex(order => new { order.Status, order.PlacedAt });
    }
}