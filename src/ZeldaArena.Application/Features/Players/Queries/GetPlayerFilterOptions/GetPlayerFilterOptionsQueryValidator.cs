using FluentValidation;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;

/// <summary>Параметров нет; валидатор — ради соглашения «запрос + хендлер + валидатор».</summary>
public sealed class GetPlayerFilterOptionsQueryValidator : AbstractValidator<GetPlayerFilterOptionsQuery>;