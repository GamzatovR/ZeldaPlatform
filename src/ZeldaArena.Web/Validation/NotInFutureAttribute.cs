using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute()
        : base("Дата не может быть в будущем.")
    {
    }

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public override bool IsValid(object? value) =>
        value is not DateOnly date || date <= Today;
}