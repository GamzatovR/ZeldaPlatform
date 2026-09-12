using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.UpdateTeamByAdmin;

public sealed record UpdateTeamByAdminCommand : ICommand, IAuditableRequest, ITeamFields
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Tag { get; init; }

    public required string CountryCode { get; init; }

    public Region Region { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    public int Rating { get; init; }

    public FileUpload? Logo { get; init; }

    public bool RemoveLogo { get; init; }

    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => Id.ToString();
}