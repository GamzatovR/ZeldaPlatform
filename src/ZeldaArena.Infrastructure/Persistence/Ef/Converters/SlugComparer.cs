using Microsoft.EntityFrameworkCore.ChangeTracking;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Converters;

/// <summary>
/// Без явного компаратора EF сравнивал бы объекты-значения по ссылке и считал
/// изменённым любой перечитанный Slug. Сравнение идёт по значению.
/// </summary>
public sealed class SlugComparer : ValueComparer<Slug>
{
    public SlugComparer()
        : base(
            (left, right) => left!.Value == right!.Value,
            slug => slug.Value.GetHashCode(StringComparison.Ordinal),
            slug => Slug.From(slug.Value))
    {
    }
}