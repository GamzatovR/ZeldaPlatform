using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>;