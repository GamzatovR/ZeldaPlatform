using FluentValidation;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;

/// <summary>Параметров нет; валидатор — ради соглашения «запрос + хендлер + валидатор».</summary>
public sealed class GetShopFilterOptionsQueryValidator : AbstractValidator<GetShopFilterOptionsQuery>;