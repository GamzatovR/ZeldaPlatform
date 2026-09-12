using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Commands.AddNewPlayerToMyTeam;

public sealed record AddNewPlayerToMyTeamCommand : ICommand, IAuditableRequest
{
    public Guid TeamId { get; init; }

    public required string Nickname { get; init; }

    public PlayerRole Role { get; init; }

    public required string CountryCode { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string AuditEntityType => nameof(Team);

    public string? AuditEntityId => TeamId.ToString();
}