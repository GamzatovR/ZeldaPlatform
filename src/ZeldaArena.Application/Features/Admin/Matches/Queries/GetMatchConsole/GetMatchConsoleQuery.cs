using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

/// <summary>Пульт счёта матча — <c>/admin/matches/{id}</c> (docs/SPEC.md §9.4, п. 3; §11).</summary>
public sealed record GetMatchConsoleQuery(Guid Id) : IQuery<MatchConsoleDto?>;