using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;

/// <summary>
/// Верхняя граница выборки — чтобы выпадающий список не превратился в выгрузку
/// всей базы игроков. Поиска по списку нет: это задача Фазы 8 вместе с AJAX.
/// </summary>
public sealed class GetFreeAgentsQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetFreeAgentsQuery, IReadOnlyList<FreeAgentDto>>
{
    public const int MaxCount = 100;

    public Task<IReadOnlyList<FreeAgentDto>> Handle(GetFreeAgentsQuery request, CancellationToken cancellationToken)
    {
        var roster = rosterEntries.Query();

        return queryExecutor.ToListAsync(
            players.Query()
                .Where(player => !roster.Any(entry => entry.PlayerId == player.Id && entry.LeftAt == null))
                .OrderBy(player => player.Nickname)
                .Take(MaxCount)
                .Select(player => new FreeAgentDto(player.Id, player.Nickname, player.Role, player.Country.Value)),
            cancellationToken);
    }
}