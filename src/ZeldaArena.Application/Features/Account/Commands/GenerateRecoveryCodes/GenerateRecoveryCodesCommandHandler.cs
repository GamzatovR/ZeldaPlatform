using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.GenerateRecoveryCodes;

public sealed class GenerateRecoveryCodesCommandHandler(
    ICurrentUserService currentUser,
    ITwoFactorService twoFactor)
    : IRequestHandler<GenerateRecoveryCodesCommand, Result<RecoveryCodes>>
{
    public async Task<Result<RecoveryCodes>> Handle(
        GenerateRecoveryCodesCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<RecoveryCodes>(AccountErrors.UserNotFound);
        }

        return await twoFactor
            .GenerateRecoveryCodesAsync(
                userId,
                EnableTwoFactorCommandHandler.RecoveryCodeCount,
                cancellationToken)
            .ConfigureAwait(false);
    }
}