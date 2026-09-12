using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

public sealed record GetTournamentsForAdminQuery : FilterBase, IQuery<PagedResult<AdminTournamentRowDto>>
{
    public string? Search { get; init; }

    public TournamentStatus? Status { get; init; }

    public Region? Region { get; init; }
}