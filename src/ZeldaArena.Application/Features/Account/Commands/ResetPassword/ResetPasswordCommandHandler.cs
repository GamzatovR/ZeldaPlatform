using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ResetPassword;

/// <summary>
/// Установка нового пароля по одноразовому токену из письма.
///
/// После успешной установки прочие сессии закрываются: пароль меняют в том числе
/// потому, что старый мог утечь, и оставлять открытые сессии в этот момент нельзя
/// (docs/SPEC.md §8.2). Уведомление на почту — вторая часть того же требования:
/// владелец должен узнать о смене, даже если её сделал не он.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IUserAccountService userAccounts,
    IAccountEmailSender emailSender)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userAccounts
            .FindByIdAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            // Тот же ответ, что и на испорченный токен: по нему нельзя проверить,
            // существует ли пользователь с таким идентификатором.
            return Result.Failure(AccountErrors.InvalidToken);
        }

        var reset = await userAccounts
            .ResetPasswordAsync(request.UserId, request.Token, request.NewPassword, cancellationToken)
            .ConfigureAwait(false);

        if (reset.IsFailure)
        {
            return reset;
        }

        await userAccounts
            .InvalidateOtherSessionsAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);

        await emailSender
            .SendPasswordChangedNoticeAsync(user.Email, user.DisplayName, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}