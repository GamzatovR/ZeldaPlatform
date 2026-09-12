using MediatR;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;

public sealed class IsEmailAvailableQueryHandler(IUserAccountService userAccounts)
    : IRequestHandler<IsEmailAvailableQuery, bool>
{
    public async Task<bool> Handle(IsEmailAvailableQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await userAccounts
            .FindByEmailAsync(request.Email.Trim(), cancellationToken)
            .ConfigureAwait(false);

        return existing is null;
    }
}