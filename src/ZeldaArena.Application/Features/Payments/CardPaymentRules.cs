using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Payments;

public static class CardPaymentRules
{
    public const int MaxIdempotencyKeyLength = PaymentIdempotency.MaxClientKeyLength;

    public static void AddCardPaymentRules<T>(this AbstractValidator<T> validator)
        where T : ICardPaymentDetails
    {
        ArgumentNullException.ThrowIfNull(validator);

        validator.RuleFor(command => command.CardNumber)
            .NotEmpty()
            .WithMessage("Укажите номер карты.")
            .Must(CardNumber.PassesLuhn)
            .WithMessage("Номер карты указан неверно.");

        validator.RuleFor(command => command.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Месяц окончания срока — число от 1 до 12.");

        validator.RuleFor(command => command.ExpiryYear)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Год окончания срока указан неверно.");

        // Срок проверяется целиком, а не по годам и месяцам порознь: декабрь
        // прошлого года проходит обе отдельные проверки.
        validator.RuleFor(command => command)
            .Must(command => !IsExpired(command.ExpiryMonth, command.ExpiryYear))
            .WithName(nameof(ICardPaymentDetails.ExpiryYear))
            .WithMessage("Срок действия карты истёк.")
            .When(command => command.ExpiryMonth is >= 1 and <= 12
                && command.ExpiryYear is >= 2000 and <= 2100);

        validator.RuleFor(command => command.Cvv)
            .NotEmpty()
            .WithMessage("Укажите CVV.")
            .Must(cvv => cvv is not null
                && cvv.Length == CardNumber.CvvLength
                && cvv.All(char.IsAsciiDigit))
            .WithMessage($"CVV состоит из {CardNumber.CvvLength} цифр.");

        validator.RuleFor(command => command.ConfirmationEmail).ValidAccountEmail();

        validator.RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Форма отправлена без ключа идемпотентности.")
            .MaximumLength(MaxIdempotencyKeyLength)
            .WithMessage($"Ключ идемпотентности не длиннее {MaxIdempotencyKeyLength} символов.");
    }

    public static bool IsExpired(int month, int year) =>
        month is < 1 or > 12
        || year is < 1 or > 9998
        || new DateOnly(year, month, 1).AddMonths(1) <= DateOnly.FromDateTime(DateTime.UtcNow);
}