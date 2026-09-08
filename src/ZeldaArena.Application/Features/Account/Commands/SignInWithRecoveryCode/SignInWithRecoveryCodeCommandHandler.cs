using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;

/// <summary>Вход по одноразовому коду восстановления.</summary>
public sealed class SignInWithRecoveryCodeCommandHandler(
    ISignInService signInService,
    IUserAccountService userAccounts)
    : IRequestHandler<SignInWithRecoveryCodeCommand, Result>
{
    public async Task<Result> Handle(
        SignInWithRecoveryCodeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await signInService.GetTwoFactorUserAsync(cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.TwoFactorSessionExpired);
        }

        var outcome = await signInService
            .RecoveryCodeSignInAsync(request.RecoveryCode, cancellationToken)
            .ConfigureAwait(false);

        if (outcome != SignInOutcome.Succeeded)
        {
            return Result.Failure(outcome == SignInOutcome.LockedOut
                ? AccountErrors.LockedOut
                : AccountErrors.RecoveryCodeInvalid);
        }

        await userAccounts.RecordSignInAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}