namespace ZeldaArena.Application.Common.Models.Billing;

/// <summary>
/// Свежевыпущенный код подтверждения и его хеш (docs/SPEC.md §7.6).
///
/// Тип живёт ровно один вызов: <see cref="Hash"/> уходит в <c>Payment</c>,
/// <see cref="Code"/> — в письмо, после чего исходный код не существует нигде,
/// кроме почтового ящика пользователя. Имя свойства подобрано так, чтобы попасть
/// под фрагмент <c>confirmationcode</c> из <c>SensitiveProperties</c>, если тип
/// когда-нибудь окажется частью запроса.
/// </summary>
public sealed record ConfirmationCode(string Code, string Hash);