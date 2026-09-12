using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchFormOptions;

/// <summary>
/// Наполнение формы назначения матча: турниры, куда можно ставить матчи, и участники
/// выбранного турнира.
///
/// Турнир выбирается первым шагом, команды — вторым, потому что играть могут только
/// участники этого турнира. Без JavaScript это обычная перезагрузка формы с турниром
/// в адресе (§9.1, прогрессивное улучшение).
/// </summary>
public sealed record GetMatchFormOptionsQuery(Guid? TournamentId) : IQuery<MatchFormOptionsDto>;