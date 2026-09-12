using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class ProductConfiguration : EntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.Property(product => product.Slug).IsRequired();
        builder.HasIndex(product => product.Slug).IsUnique();

        builder.Property(product => product.Sku).IsRequired().HasMaxLength(40);
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Name).IsRequired().HasMaxLength(200);
        builder.Property(product => product.Description).HasMaxLength(4000);
        builder.Property(product => product.ImagePath).HasMaxLength(400);

        builder.Ignore(product => product.IsInStock);

        builder.ComplexProperty(product => product.Price, money =>
        {
            money.Property(amount => amount.Amount).HasColumnName("Price");
            money.Property(currency => currency.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength();
        });

        // Остаток и цена меняются конкурентно администратором и покупателями.
        builder.UseXminAsConcurrencyToken();

        builder.HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индекс под фильтр магазина: категория, наличие, цена.
        builder.HasIndex(product => new { product.CategoryId, product.IsActive });
        builder.HasIndex(product => product.IsActive);
    }
}