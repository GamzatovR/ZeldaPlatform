using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.SignIn;

/// <summary>
/// Вход по паролю (docs/SPEC.md §8.2).
///
/// Успехом считаются два исхода: вошли и «нужен второй фактор». Остальное —
/// неудача с кодом ошибки, и это не формальность: <c>AuditBehavior</c> пишет
/// неуспешный результат в журнал, а неудачный вход обязан там появиться (§8.2).
///
/// Причина неудачи наружу не детализируется, кроме блокировок: и неверный пароль,
/// и незнакомый адрес дают один и тот же ответ.
/// </summary>
public sealed class SignInCommandHandler(
    ISignInService signInService,
    IUserAccountService userAccounts)
    : IRequestHandler<SignInCommand, Result<SignInOutcome>>
{
    public async Task<Result<SignInOutcome>> Handle(
        SignInCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userAccounts
            .FindByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        // Блокировка администратором проверяется до пароля. Так заблокированный
        // не получит cookie даже на время одного запроса. Плата известна: по ответу
        // видно, что такой адрес существует. Для бессрочной блокировки это приемлемо —
        // обычная неудача входа по-прежнему неотличима от незнакомого адреса.
        if (user is { IsBlocked: true })
        {
            return Result.Failure<SignInOutcome>(AccountErrors.Blocked);
        }

        var outcome = await signInService
            .PasswordSignInAsync(request.Email, request.Password, request.RememberMe, cancellationToken)
            .ConfigureAwait(false);

        if (outcome == SignInOutcome.Succeeded && user is not null)
        {
            await userAccounts.RecordSignInAsync(user.Id, cancellationToken).ConfigureAwait(false);
        }

        return outcome switch
        {
            SignInOutcome.Succeeded or SignInOutcome.RequiresTwoFactor => Result.Success(outcome),
            SignInOutcome.LockedOut => Result.Failure<SignInOutcome>(AccountErrors.LockedOut),
            SignInOutcome.NotAllowed =>
                Result.Failure<SignInOutcome>(AccountErrors.EmailNotConfirmed),
            _ => Result.Failure<SignInOutcome>(AccountErrors.InvalidCredentials),
        };
    }
}