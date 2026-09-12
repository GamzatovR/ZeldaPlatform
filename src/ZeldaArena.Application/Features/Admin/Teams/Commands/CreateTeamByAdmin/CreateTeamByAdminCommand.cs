using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.CreateTeamByAdmin;

/// <summary>
/// Команда, заведённая администрацией (docs/SPEC.md §9.4, п. 4). В отличие от команды
/// подписчика она сразу одобрена и не имеет владельца: <c>Team.Create</c> против
/// <c>Team.CreateByUser</c>.
/// </summary>
public sealed record CreateTeamByAdminCommand : ICommand<Guid>, IAuditableRequest, ITeamFields
{
    public required string Name { get; init; }

    public required string Tag { get; init; }

    public required string CountryCode { get; init; }

    public Region Region { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    public int Rating { get; init; }

    public FileUpload? Logo { get; init; }

    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => null;
}