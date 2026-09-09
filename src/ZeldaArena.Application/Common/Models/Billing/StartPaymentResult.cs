namespace ZeldaArena.Application.Common.Models.Billing;

/// <summary>
/// Ответ на создание платежа (docs/SPEC.md §7.6, шаг 2): идентификатор, по которому
/// подтверждают оплату, и маскированный адрес, чтобы страница подсказала, куда
/// смотреть. Больше о карте не возвращается ничего.
/// </summary>
public sealed record StartPaymentResult(Guid PaymentId, string MaskedEmail);