using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ChangePassword;

/// <summary>
/// Смена пароля пользователем, который уже вошёл.
///
/// Порядок шагов важен. Сначала меняем пароль, затем закрываем прочие сессии
/// и только потом перевыписываем cookie текущему пользователю: иначе он разлогинил бы
/// сам себя вместе с остальными (docs/SPEC.md §8.2).
///
/// Стамп безопасности сбрасывается явно, хотя UserManager делает это и сам:
/// требование §8.2 не должно зависеть от внутреннего устройства Identity.
/// </summary>
public sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts,
    ISignInService signInService,
    IAccountEmailSender emailSender)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var user = await userAccounts.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var changed = await userAccounts
            .ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken)
            .ConfigureAwait(false);

        if (changed.IsFailure)
        {
            return changed;
        }

        await userAccounts.InvalidateOtherSessionsAsync(userId, cancellationToken).ConfigureAwait(false);
        await signInService.RefreshSignInAsync(userId, cancellationToken).ConfigureAwait(false);

        await emailSender
            .SendPasswordChangedNoticeAsync(user.Email, user.DisplayName, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}