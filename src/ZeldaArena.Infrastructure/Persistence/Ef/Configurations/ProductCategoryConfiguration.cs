using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class ProductCategoryConfiguration : EntityConfiguration<ProductCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.Property(category => category.Slug).IsRequired();
        builder.HasIndex(category => category.Slug).IsUnique();

        builder.Property(category => category.Name).IsRequired().HasMaxLength(120);
    }
}