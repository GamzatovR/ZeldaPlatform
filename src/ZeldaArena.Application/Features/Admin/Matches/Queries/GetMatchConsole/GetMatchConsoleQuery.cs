using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

/// <summary>Пульт счёта матча — <c>/admin/matches/{id}</c>.</summary>
public sealed record GetMatchConsoleQuery(Guid Id) : IQuery<MatchConsoleDto?>;