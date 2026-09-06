using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Converters;

/// <summary>
/// Slug хранится обычной строкой: уникальный индекс и поиск по слагу работают
/// без дополнительных типов на стороне PostgreSQL.
/// </summary>
public sealed class SlugConverter : ValueConverter<Slug, string>
{
    public SlugConverter()
        : base(slug => slug.Value, value => Slug.From(value))
    {
    }
}