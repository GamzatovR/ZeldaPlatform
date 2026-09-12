using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Billing;

namespace ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;

public sealed record GetPaymentStateQuery(Guid PaymentId) : IQuery<PaymentStateDto?>;