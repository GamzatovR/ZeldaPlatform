using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithTwoFactor;

public sealed class SignInWithTwoFactorCommandHandler(
    ISignInService signInService,
    IUserAccountService userAccounts)
    : IRequestHandler<SignInWithTwoFactorCommand, Result>
{
    public async Task<Result> Handle(
        SignInWithTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await signInService.GetTwoFactorUserAsync(cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.TwoFactorSessionExpired);
        }

        var outcome = await signInService
            .TwoFactorSignInAsync(
                request.Code,
                request.RememberMe,
                request.RememberDevice,
                cancellationToken)
            .ConfigureAwait(false);

        if (outcome != SignInOutcome.Succeeded)
        {
            return Result.Failure(outcome == SignInOutcome.LockedOut
                ? AccountErrors.LockedOut
                : AccountErrors.TwoFactorCodeInvalid);
        }

        await userAccounts.RecordSignInAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}