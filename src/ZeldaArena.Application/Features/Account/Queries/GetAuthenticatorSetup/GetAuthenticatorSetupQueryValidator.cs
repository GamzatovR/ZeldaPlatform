using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;

public sealed class GetAuthenticatorSetupQueryValidator
    : AbstractValidator<GetAuthenticatorSetupQuery>;