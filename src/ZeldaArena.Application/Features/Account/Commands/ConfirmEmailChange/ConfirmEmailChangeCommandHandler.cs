using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmailChange;

public sealed class ConfirmEmailChangeCommandHandler(IUserAccountService userAccounts)
    : IRequestHandler<ConfirmEmailChangeCommand, Result>
{
    public async Task<Result> Handle(
        ConfirmEmailChangeCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var changed = await userAccounts
            .ChangeEmailAsync(request.UserId, request.NewEmail, request.Token, cancellationToken)
            .ConfigureAwait(false);

        if (changed.IsFailure)
        {
            return changed;
        }

        return await userAccounts
            .InvalidateOtherSessionsAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);
    }
}