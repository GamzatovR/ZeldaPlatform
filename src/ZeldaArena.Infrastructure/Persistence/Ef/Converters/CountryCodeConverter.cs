using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Converters;

public sealed class CountryCodeConverter : ValueConverter<CountryCode, string>
{
    public CountryCodeConverter()
        : base(country => country.Value, value => CountryCode.From(value))
    {
    }
}