using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

/// <summary>Карточка турнира для правки: поля, участники, команды, которых можно добавить.</summary>
public sealed record GetTournamentForEditQuery(Guid Id) : IQuery<TournamentEditDto?>;