using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    ICurrentUserService currentUser,
    IUserAccountService userAccounts)
    : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        return await userAccounts
            .UpdateProfileAsync(
                userId,
                request.DisplayName,
                request.CountryCode,
                request.PreferredCulture,
                cancellationToken)
            .ConfigureAwait(false);
    }
}