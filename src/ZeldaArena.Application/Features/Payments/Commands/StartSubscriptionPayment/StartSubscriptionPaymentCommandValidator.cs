using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

/// <summary>
/// Серверная половина двухуровневой валидации реквизитов (docs/SPEC.md §7.6, шаг 1;
/// §15). Работает и при выключенном JavaScript — это отдельный пункт чек-листа §19.
///
/// Сообщения конкретны намеренно: «карта не подходит» заставляет пользователя
/// перебирать поля вслепую.
/// </summary>
public sealed class StartSubscriptionPaymentCommandValidator
    : AbstractValidator<StartSubscriptionPaymentCommand>
{
    /// <summary>
    /// Клиентская часть ключа идемпотентности; форма присылает Guid. Длина ограничена
    /// так, чтобы вместе с идентификатором владельца ключ помещался в столбец
    /// (<see cref="PaymentIdempotency"/>, docs/SPEC.md §6).
    /// </summary>
    public const int MaxIdempotencyKeyLength = PaymentIdempotency.MaxClientKeyLength;

    public StartSubscriptionPaymentCommandValidator()
    {
        RuleFor(command => command.PlanId)
            .NotEmpty()
            .WithMessage("Выберите тариф.");

        RuleFor(command => command.CardNumber)
            .NotEmpty()
            .WithMessage("Укажите номер карты.")
            .Must(CardNumber.PassesLuhn)
            .WithMessage("Номер карты указан неверно.");

        RuleFor(command => command.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Месяц окончания срока — число от 1 до 12.");

        RuleFor(command => command.ExpiryYear)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Год окончания срока указан неверно.");

        // Срок проверяется целиком, а не по годам и месяцам порознь: декабрь
        // прошлого года проходит обе отдельные проверки.
        RuleFor(command => command)
            .Must(NotExpired)
            .WithName(nameof(StartSubscriptionPaymentCommand.ExpiryYear))
            .WithMessage("Срок действия карты истёк.")
            .When(command => command.ExpiryMonth is >= 1 and <= 12
                && command.ExpiryYear is >= 2000 and <= 2100);

        RuleFor(command => command.Cvv)
            .NotEmpty()
            .WithMessage("Укажите CVV.")
            .Must(cvv => cvv is not null
                && cvv.Length == CardNumber.CvvLength
                && cvv.All(char.IsAsciiDigit))
            .WithMessage($"CVV состоит из {CardNumber.CvvLength} цифр.");

        RuleFor(command => command.ConfirmationEmail).ValidAccountEmail();

        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Форма отправлена без ключа идемпотентности.")
            .MaximumLength(MaxIdempotencyKeyLength)
            .WithMessage($"Ключ идемпотентности не длиннее {MaxIdempotencyKeyLength} символов.");
    }

    /// <summary>
    /// Карта действительна до последнего дня указанного месяца включительно.
    /// Сегодняшний день берётся у системных часов, потому что валидатор не должен
    /// зависеть от порта времени: правило про календарь, а не про состояние приложения.
    /// </summary>
    private static bool NotExpired(StartSubscriptionPaymentCommand command) =>
        new DateOnly(command.ExpiryYear, command.ExpiryMonth, 1).AddMonths(1)
        > DateOnly.FromDateTime(DateTime.UtcNow);
}