using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Commands.GenerateRecoveryCodes;

public sealed class GenerateRecoveryCodesCommandValidator
    : AbstractValidator<GenerateRecoveryCodesCommand>;