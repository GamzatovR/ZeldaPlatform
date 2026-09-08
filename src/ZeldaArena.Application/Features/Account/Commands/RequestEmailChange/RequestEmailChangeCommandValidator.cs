using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.RequestEmailChange;

public sealed class RequestEmailChangeCommandValidator
    : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator() =>
        RuleFor(command => command.NewEmail).ValidAccountEmail();
}