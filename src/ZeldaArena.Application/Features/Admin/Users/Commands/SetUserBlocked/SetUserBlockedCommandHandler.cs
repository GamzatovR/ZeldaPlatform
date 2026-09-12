using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;

public sealed class SetUserBlockedCommandHandler(
    IUserAdministrationService users,
    ICurrentUserService currentUser)
    : IRequestHandler<SetUserBlockedCommand, Result>
{
    public Task<Result> Handle(SetUserBlockedCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.UserId == currentUser.UserId
            ? Task.FromResult(Result.Failure(AccountErrors.CannotBlockSelf))
            : users.SetBlockedAsync(request.UserId, request.IsBlocked, cancellationToken);
    }
}