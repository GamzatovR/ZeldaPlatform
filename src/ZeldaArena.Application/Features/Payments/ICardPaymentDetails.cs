namespace ZeldaArena.Application.Features.Payments;

/// <summary>
/// Реквизиты мнимой оплаты (docs/SPEC.md §7.6, шаг 1) — общие для подписки и заказа.
/// Команда, реализующая этот контракт, получает одни и те же правила валидации
/// (<see cref="CardPaymentRules"/>) и один и тот же путь заведения платежа
/// (<see cref="PaymentInitiator"/>): две копии разъехались бы, и форма заказа начала бы
/// принимать карту, которую отвергает форма подписки.
///
/// Номер карты и CVV живут ровно до вызова платёжного провайдера; в аудит они не попадут,
/// потому что имена свойств закрыты <c>SensitiveProperties</c>.
/// </summary>
public interface ICardPaymentDetails
{
    string CardNumber { get; }

    int ExpiryMonth { get; }

    int ExpiryYear { get; }

    string Cvv { get; }

    /// <summary>Куда уйдёт письмо с кодом и чек.</summary>
    string ConfirmationEmail { get; }

    /// <summary>Клиентская часть ключа идемпотентности, см. <see cref="PaymentIdempotency"/>.</summary>
    string IdempotencyKey { get; }
}