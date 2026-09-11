using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.UpdateMyTeamProfile;

/// <summary>Правка профиля своей команды капитаном (docs/SPEC.md §9.3, п. 8).</summary>
public sealed record UpdateMyTeamProfileCommand : ICommand, IAuditableRequest
{
    public Guid TeamId { get; init; }

    public required string Name { get; init; }

    public required string Tag { get; init; }

    public required string CountryCode { get; init; }

    public Region Region { get; init; }

    public DateOnly? FoundedAt { get; init; }

    public string? Description { get; init; }

    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}