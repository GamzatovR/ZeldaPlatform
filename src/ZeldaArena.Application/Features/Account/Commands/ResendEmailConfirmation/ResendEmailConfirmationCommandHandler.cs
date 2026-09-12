using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationCommandHandler(
    IUserAccountService userAccounts,
    IAccountEmailSender emailSender)
    : IRequestHandler<ResendEmailConfirmationCommand, Result>
{
    public async Task<Result> Handle(
        ResendEmailConfirmationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userAccounts
            .FindByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || user.EmailConfirmed)
        {
            return Result.Success();
        }

        var token = await userAccounts
            .GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);

        if (token.IsFailure)
        {
            return Result.Failure(token.Error);
        }

        await emailSender
            .SendEmailConfirmationAsync(
                user.Email,
                user.DisplayName,
                user.Id,
                token.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}