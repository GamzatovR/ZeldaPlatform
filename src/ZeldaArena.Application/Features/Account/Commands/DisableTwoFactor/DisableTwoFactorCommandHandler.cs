using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorCommandHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts,
    ITwoFactorService twoFactor,
    ISignInService signInService)
    : IRequestHandler<DisableTwoFactorCommand, Result>
{
    public static readonly Error AdminMustKeepTwoFactor =
        new("account.two_factor_required_for_admin",
            "Для роли Admin двухфакторная аутентификация обязательна.");

    public async Task<Result> Handle(
        DisableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var user = await userAccounts.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        if (user.Roles.Contains(RoleNames.Admin, StringComparer.Ordinal))
        {
            return Result.Failure(AdminMustKeepTwoFactor);
        }

        var disabled = await twoFactor.DisableAsync(userId, cancellationToken).ConfigureAwait(false);

        if (disabled.IsFailure)
        {
            return disabled;
        }

        await signInService.RefreshSignInAsync(userId, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}