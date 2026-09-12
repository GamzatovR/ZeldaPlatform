using FluentValidation;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;

/// <summary>
/// Параметров у запроса нет, но сценарий без валидатора нарушил бы соглашение
/// «команда/запрос + хендлер + валидатор» (docs/CONVENTIONS.md).
/// </summary>
public sealed class GetTournamentOptionsQueryValidator : AbstractValidator<GetTournamentOptionsQuery>;