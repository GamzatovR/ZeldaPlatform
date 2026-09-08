using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;

/// <summary>
/// Включает второй фактор и сразу выдаёт коды восстановления.
///
/// Коды выпускаются в том же сценарии не для удобства: без них пользователь,
/// потерявший телефон, теряет доступ насовсем. Показать их можно только здесь.
///
/// Cookie перевыписывается: в ней хранится признак прохождения второго фактора,
/// и без обновления текущая сессия оказалась бы в подвешенном состоянии.
/// </summary>
public sealed class EnableTwoFactorCommandHandler(
    ICurrentUserService currentUser,
    ITwoFactorService twoFactor,
    ISignInService signInService)
    : IRequestHandler<EnableTwoFactorCommand, Result<RecoveryCodes>>
{
    /// <summary>Десять кодов по docs/SPEC.md §8.2.</summary>
    public const int RecoveryCodeCount = 10;

    public async Task<Result<RecoveryCodes>> Handle(
        EnableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<RecoveryCodes>(AccountErrors.UserNotFound);
        }

        var enabled = await twoFactor
            .EnableAsync(userId, request.VerificationCode, cancellationToken)
            .ConfigureAwait(false);

        if (enabled.IsFailure)
        {
            return Result.Failure<RecoveryCodes>(enabled.Error);
        }

        await signInService.RefreshSignInAsync(userId, cancellationToken).ConfigureAwait(false);

        return await twoFactor
            .GenerateRecoveryCodesAsync(userId, RecoveryCodeCount, cancellationToken)
            .ConfigureAwait(false);
    }
}