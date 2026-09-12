using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

public sealed record GetMatchesForAdminQuery : FilterBase, IQuery<PagedResult<AdminMatchRowDto>>
{
    public Guid? TournamentId { get; init; }

    public MatchStatus? Status { get; init; }
}