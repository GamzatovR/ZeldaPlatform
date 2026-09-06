using Microsoft.EntityFrameworkCore.ChangeTracking;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Converters;

public sealed class CountryCodeComparer : ValueComparer<CountryCode>
{
    public CountryCodeComparer()
        : base(
            (left, right) => left!.Value == right!.Value,
            country => country.Value.GetHashCode(StringComparison.Ordinal),
            country => CountryCode.From(country.Value))
    {
    }
}