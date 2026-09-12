using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IUserAccountService userAccounts,
    IAccountEmailSender emailSender)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userAccounts
            .FindByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        // Неподтверждённый адрес не обслуживается: иначе сброс пароля стал бы обходным
        // путём вокруг подтверждения почты.
        if (user is null || !user.EmailConfirmed)
        {
            return Result.Success();
        }

        var token = await userAccounts
            .GeneratePasswordResetTokenAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);

        if (token.IsFailure)
        {
            return Result.Failure(token.Error);
        }

        await emailSender
            .SendPasswordResetAsync(user.Email, user.DisplayName, user.Id, token.Value, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}