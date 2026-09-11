using FluentValidation;

namespace ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

/// <summary>Параметров нет; валидатор — ради соглашения «запрос + хендлер + валидатор».</summary>
public sealed class GetMiniCartQueryValidator : AbstractValidator<GetMiniCartQuery>;