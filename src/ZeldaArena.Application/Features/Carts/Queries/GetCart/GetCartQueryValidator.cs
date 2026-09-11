using FluentValidation;

namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>Параметров нет; валидатор — ради соглашения «запрос + хендлер + валидатор».</summary>
public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>;