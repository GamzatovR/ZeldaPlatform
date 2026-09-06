using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

/// <summary>
/// Токен конкурентности для PostgreSQL. Аналога SQL Server rowversion в PostgreSQL нет,
/// зато есть системный столбец xmin — идентификатор транзакции, изменившей строку.
/// Он объявлен теневым свойством, поэтому столбца RowVersion в доменных сущностях нет:
/// инфраструктурная деталь не протекает в Domain (docs/adr/ADR-0003).
///
/// Собственное расширение, а не UseXminAsConcurrencyToken провайдера: этот метод
/// удалён в Npgsql 10.
/// </summary>
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