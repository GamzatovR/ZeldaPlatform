using FluentValidation;

namespace ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

/// <summary>Параметров нет; валидатор — ради соглашения «команда + хендлер + валидатор».</summary>
public sealed class MergeGuestCartCommandValidator : AbstractValidator<MergeGuestCartCommand>;