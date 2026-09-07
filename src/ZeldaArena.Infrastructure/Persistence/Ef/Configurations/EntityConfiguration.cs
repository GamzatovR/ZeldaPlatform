using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

/// <summary>
/// Общая часть конфигурации любой сущности: первичный ключ и отсечение накопителя
/// доменных событий, который в базе не хранится. Ключ генерируется в конструкторе
/// сущности (Guid v7), поэтому база его не подставляет.
/// </summary>
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Ignore(entity => entity.DomainEvents);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}