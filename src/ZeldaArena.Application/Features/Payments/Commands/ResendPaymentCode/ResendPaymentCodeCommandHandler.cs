using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;

public sealed class ResendPaymentCodeCommandHandler(
    ICurrentUserService currentUser,
    IRepository<Payment> payments,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<ResendPaymentCodeCommand, Result>
{
    public async Task<Result> Handle(ResendPaymentCodeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var payment = await payments
            .GetByIdAsync(request.PaymentId, cancellationToken)
            .ConfigureAwait(false);

        if (payment is null || payment.UserId != userId)
        {
            return Result.Failure(BillingErrors.PaymentNotFound);
        }

        if (!payment.IsPending)
        {
            return Result.Failure(BillingErrors.PaymentNotPending);
        }

        var now = clock.UtcNow;

        if (!PaymentTiming.CanResendAt(payment, now))
        {
            return Result.Failure(BillingErrors.ResendTooSoon);
        }

        var code = codes.Issue();

        payment.ReissueCode(code.Hash, now.Add(PaymentPolicy.CodeLifetime));

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await emailSender
            .SendPaymentCodeAsync(
                payment.ConfirmationEmail,
                currentUser.UserName,
                code.Code,
                payment.ConfirmationExpiresAt,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}