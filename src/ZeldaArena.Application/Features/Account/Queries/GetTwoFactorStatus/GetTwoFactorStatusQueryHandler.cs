using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Account.Queries.GetTwoFactorStatus;

public sealed class GetTwoFactorStatusQueryHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts,
    ITwoFactorService twoFactor)
    : IRequestHandler<GetTwoFactorStatusQuery, TwoFactorStatusDto?>
{
    public async Task<TwoFactorStatusDto?> Handle(
        GetTwoFactorStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return null;
        }

        var user = await userAccounts.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        var remaining = await twoFactor
            .CountRemainingRecoveryCodesAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return new TwoFactorStatusDto(
            user.TwoFactorEnabled,
            remaining.IsSuccess ? remaining.Value : 0,
            user.Roles.Contains(RoleNames.Admin, StringComparer.Ordinal));
    }
}