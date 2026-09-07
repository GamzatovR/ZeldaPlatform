using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Common.Entities;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class ContentTranslationConfiguration : EntityConfiguration<ContentTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ContentTranslation> builder)
    {
        builder.ToTable("ContentTranslations");

        builder.Property(translation => translation.EntityType).IsRequired().HasMaxLength(60);
        builder.Property(translation => translation.CultureCode).IsRequired().HasMaxLength(10);
        builder.Property(translation => translation.FieldName).IsRequired().HasMaxLength(60);
        builder.Property(translation => translation.Value).IsRequired();

        // Один перевод на связку «сущность + запись + язык + поле». Id в индекс не входит:
        // суррогатный ключ уникален сам по себе и в ограничении бесполезен.
        builder.HasIndex(translation => new
        {
            translation.EntityType,
            translation.EntityId,
            translation.CultureCode,
            translation.FieldName,
        }).IsUnique();
    }
}