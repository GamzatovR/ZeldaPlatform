using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Реквизиты карты для мнимой авторизации (docs/SPEC.md §7.6).
///
/// ВАЖНО. Этот тип живёт ровно один вызов: от хендлера до <c>IPaymentGateway</c>.
/// Полный номер карты и CVV не сохраняются в базу, не пишутся в логи, не попадают
/// в аудит и не возвращаются клиенту — наружу выходит только
/// <see cref="CardAuthorization"/> с брендом и последними четырьмя цифрами.
/// Имена свойств перечислены в <c>SensitiveProperties</c>, чтобы behaviors
/// не сериализовали их по недосмотру.
/// </summary>
public sealed record CardPaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    Money Amount);