using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class CartConfiguration : EntityConfiguration<Cart>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.Ignore(cart => cart.IsEmpty);
        builder.Ignore(cart => cart.TotalQuantity);
        builder.Ignore(cart => cart.Subtotal);

        // У пользователя одна корзина; гостевая опознаётся по подписанной куке.
        builder.HasIndex(cart => cart.UserId).IsUnique().HasFilter("\"UserId\" IS NOT NULL");
        builder.HasIndex(cart => cart.AnonymousId).IsUnique().HasFilter("\"AnonymousId\" IS NOT NULL");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cart => cart.Items)
            .WithOne()
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Позиции — часть агрегата: по ним Cart.AddItem решает, увеличить ли количество
        // или завести строку. Без AutoInclude репозиторий отдавал бы корзину пустой,
        // и повторное добавление падало бы на уникальном (CartId, ProductId) — ровно
        // так в Фазе 4 ломалась привязка фич к тарифу.
        builder.Navigation(cart => cart.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}