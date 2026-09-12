using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;

public sealed class GetAccountProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts)
    : IRequestHandler<GetAccountProfileQuery, UserAccountDto?>
{
    public Task<UserAccountDto?> Handle(
        GetAccountProfileQuery request,
        CancellationToken cancellationToken) =>
        currentUser.UserId is { } userId
            ? userAccounts.FindByIdAsync(userId, cancellationToken)
            : Task.FromResult<UserAccountDto?>(null);
}