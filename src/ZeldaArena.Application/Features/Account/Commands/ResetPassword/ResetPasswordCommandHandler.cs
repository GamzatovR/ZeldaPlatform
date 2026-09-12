using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ResetPassword;

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