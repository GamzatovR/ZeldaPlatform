using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Converters;

public sealed class SlugConverter : ValueConverter<Slug, string>
{
    public SlugConverter()
        : base(slug => slug.Value, value => Slug.From(value))
    {
    }
}