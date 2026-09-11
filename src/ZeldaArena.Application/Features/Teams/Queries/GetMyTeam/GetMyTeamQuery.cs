using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

/// <summary>
/// Кабинет капитана (docs/SPEC.md §9.3, п. 8, <c>/account/my-team</c>): своя команда, её состав
/// и можно ли её сейчас править.
///
/// Читать кабинет можно всегда, править — только с функцией <c>team.create</c>:
/// после истечения подписки команда остаётся, но редактирование блокируется (CLAUDE.md).
/// </summary>
/// <param name="TeamId">
/// Какую из своих команд открыть. Пусто — первую: лимит команд — параметр тарифа (EP-5),
/// и своих команд может оказаться больше одной.
/// </param>
public sealed record GetMyTeamQuery(Guid? TeamId = null) : IQuery<MyTeamDto?>;