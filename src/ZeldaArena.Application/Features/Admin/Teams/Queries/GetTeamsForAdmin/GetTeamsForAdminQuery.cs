using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

public sealed record GetTeamsForAdminQuery : FilterBase, IQuery<PagedResult<AdminTeamRowDto>>
{
    public string? Search { get; init; }

    public Region? Region { get; init; }

    /// <summary><see langword="true"/> — только ждущие одобрения, <see langword="false"/> — только одобренные.</summary>
    public bool? IsApproved { get; init; }
}