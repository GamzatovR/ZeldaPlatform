using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;

/// <summary>
/// Отдаёт состояние платежа его владельцу и никому больше (docs/SPEC.md §15,
/// защита от IDOR): чужой платёж отвечает так же, как несуществующий.
///
/// Ни кода, ни его хеша, ни номера карты в ответе нет — только маскированный адрес
/// и счётчики, нужные форме.
/// </summary>
public sealed class GetPaymentStateQueryHandler(
    ICurrentUserService currentUser,
    IReadRepository<Payment> payments,
    IReadRepository<Order> orders,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock)
    : IRequestHandler<GetPaymentStateQuery, PaymentStateDto?>
{
    public async Task<PaymentStateDto?> Handle(
        GetPaymentStateQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var payment = await payments
            .FindAsync(request.PaymentId, cancellationToken)
            .ConfigureAwait(false);

        if (payment is null || payment.UserId != userId)
        {
            return null;
        }

        var orderNumber = payment.OrderId is { } orderId
            ? await queryExecutor
                .FirstOrDefaultAsync(
                    orders.Query().Where(order => order.Id == orderId).Select(order => order.Number),
                    cancellationToken)
                .ConfigureAwait(false)
            : null;

        return new PaymentStateDto(
            payment.Id,
            payment.Status,
            MaskedEmail.Of(payment.ConfirmationEmail),
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.ConfirmationAttemptsLeft,
            payment.ConfirmationExpiresAt,
            PaymentTiming.CanResendAt(payment, clock.UtcNow),
            payment.Purpose,
            orderNumber);
    }
}