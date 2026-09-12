using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;

public sealed class EnableTwoFactorCommandHandler(
    ICurrentUserService currentUser,
    ITwoFactorService twoFactor,
    ISignInService signInService)
    : IRequestHandler<EnableTwoFactorCommand, Result<RecoveryCodes>>
{
    /// <summary>Десять кодов восстановления.</summary>
    public const int RecoveryCodeCount = 10;

    public async Task<Result<RecoveryCodes>> Handle(
        EnableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<RecoveryCodes>(AccountErrors.UserNotFound);
        }

        var enabled = await twoFactor
            .EnableAsync(userId, request.VerificationCode, cancellationToken)
            .ConfigureAwait(false);

        if (enabled.IsFailure)
        {
            return Result.Failure<RecoveryCodes>(enabled.Error);
        }

        await signInService.RefreshSignInAsync(userId, cancellationToken).ConfigureAwait(false);

        return await twoFactor
            .GenerateRecoveryCodesAsync(userId, RecoveryCodeCount, cancellationToken)
            .ConfigureAwait(false);
    }
}