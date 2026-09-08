using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;

public sealed class GetAuthenticatorSetupQueryHandler(
    ICurrentUserService currentUser,
    ITwoFactorService twoFactor)
    : IRequestHandler<GetAuthenticatorSetupQuery, TwoFactorSetup?>
{
    public async Task<TwoFactorSetup?> Handle(
        GetAuthenticatorSetupQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var setup = await twoFactor.GetSetupAsync(userId, cancellationToken).ConfigureAwait(false);

        return setup.IsSuccess ? setup.Value : null;
    }
}