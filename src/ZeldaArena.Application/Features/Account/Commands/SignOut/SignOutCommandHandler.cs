using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.SignOut;

/// <summary>
/// Удаляет cookie аутентификации. Отдельная команда, а не вызов SignOutAsync
/// прямо со страницы, нужна ради записи в аудит: без неё в журнале были бы входы
/// без выходов (docs/SPEC.md §8.2).
/// </summary>
public sealed class SignOutCommandHandler(ISignInService signInService)
    : IRequestHandler<SignOutCommand, Result>
{
    public async Task<Result> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        await signInService.SignOutAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}