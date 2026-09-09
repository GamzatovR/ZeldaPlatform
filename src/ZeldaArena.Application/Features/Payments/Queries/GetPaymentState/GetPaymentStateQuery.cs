using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;

namespace ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;

/// <summary>
/// Состояние платежа для страницы ввода кода (docs/SPEC.md §7.6, шаг 3): куда ушло
/// письмо, сколько осталось попыток и времени, можно ли выслать код заново.
///
/// Этим же запросом форма узнаёт остаток попыток после неверного ввода: команда
/// подтверждения возвращает только исход, а число попыток живёт в одном месте —
/// в самой записи платежа.
/// </summary>
public sealed record GetPaymentStateQuery(Guid PaymentId) : IQuery<PaymentStateDto?>;