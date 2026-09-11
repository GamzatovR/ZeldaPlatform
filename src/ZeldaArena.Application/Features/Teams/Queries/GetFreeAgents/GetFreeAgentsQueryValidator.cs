using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;

/// <summary>Параметров нет; валидатор — ради соглашения «запрос + хендлер + валидатор».</summary>
public sealed class GetFreeAgentsQueryValidator : AbstractValidator<GetFreeAgentsQuery>;