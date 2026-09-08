using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;

/// <summary>
/// Параметров нет: отключается второй фактор текущего пользователя. Валидатор заведён
/// ради единообразия срезов (CLAUDE.md) — правило «админу отключать нельзя» живёт
/// в хендлере, потому что требует обращения к ролям.
/// </summary>
public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>;