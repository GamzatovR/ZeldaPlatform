using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.RequestEmailChange;

/// <summary>
/// Запрос смены адреса. Сам адрес пока не меняется — уходят два письма
/// (docs/SPEC.md §8.2):
///
/// на новый адрес — ссылка подтверждения, потому что подтвердить смену может
/// только тот, кто этим адресом владеет; на старый — уведомление, чтобы захват
/// учётной записи не прошёл незамеченным.
/// </summary>
public sealed class RequestEmailChangeCommandHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts,
    IAccountEmailSender emailSender)
    : IRequestHandler<RequestEmailChangeCommand, Result>
{
    public async Task<Result> Handle(
        RequestEmailChangeCommand request,
        CancellationToken cancellationToken)
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

        if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            // Смена на тот же адрес — не ошибка ввода, а бессмысленное действие:
            // отвечаем успехом, писем не шлём.
            return Result.Success();
        }

        var taken = await userAccounts
            .FindByEmailAsync(request.NewEmail, cancellationToken)
            .ConfigureAwait(false);

        if (taken is not null)
        {
            return Result.Failure(AccountErrors.EmailAlreadyTaken);
        }

        var token = await userAccounts
            .GenerateEmailChangeTokenAsync(userId, request.NewEmail, cancellationToken)
            .ConfigureAwait(false);

        if (token.IsFailure)
        {
            return Result.Failure(token.Error);
        }

        await emailSender
            .SendEmailChangeConfirmationAsync(
                request.NewEmail,
                user.DisplayName,
                userId,
                token.Value,
                cancellationToken)
            .ConfigureAwait(false);

        await emailSender
            .SendEmailChangedNoticeAsync(
                user.Email,
                user.DisplayName,
                request.NewEmail,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}