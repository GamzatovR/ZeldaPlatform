namespace ZeldaArena.Application.Features.Payments;

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