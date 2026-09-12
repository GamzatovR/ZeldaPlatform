using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public static class ConcurrencyTokenExtensions
{
    public static EntityTypeBuilder<TEntity> UseXminAsConcurrencyToken<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        return builder;
    }
}