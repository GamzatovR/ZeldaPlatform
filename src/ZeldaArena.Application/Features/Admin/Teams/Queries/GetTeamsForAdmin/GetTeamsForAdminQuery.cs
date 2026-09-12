using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

/// <summary>
/// Таблица команд — <c>/admin/teams</c> (docs/SPEC.md §9.4, п. 4). В отличие от списка
/// на сайте, показывает и неодобренные команды подписчиков: одобрять их — работа
/// этой страницы.
/// </summary>
public sealed record GetTeamsForAdminQuery : FilterBase, IQuery<PagedResult<AdminTeamRowDto>>
{
    public string? Search { get; init; }

    public Region? Region { get; init; }

    /// <summary><see langword="true"/> — только ждущие одобрения, <see langword="false"/> — только одобренные.</summary>
    public bool? IsApproved { get; init; }
}