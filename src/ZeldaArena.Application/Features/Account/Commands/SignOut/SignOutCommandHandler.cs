using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Account.Commands.SignOut;

public sealed class SignOutCommandHandler(ISignInService signInService)
    : IRequestHandler<SignOutCommand, Result>
{
    public async Task<Result> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        await signInService.SignOutAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}