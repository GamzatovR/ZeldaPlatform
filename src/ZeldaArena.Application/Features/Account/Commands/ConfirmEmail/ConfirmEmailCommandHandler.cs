using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(IUserAccountService userAccounts)
    : IRequestHandler<ConfirmEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await userAccounts
            .ConfirmEmailAsync(request.UserId, request.Token, cancellationToken)
            .ConfigureAwait(false);
    }
}