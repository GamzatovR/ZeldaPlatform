using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Commands.UpdatePlayer;

/// <summary>Правка профиля игрока. Слаг не меняется: это адрес страницы (ADR-0008).</summary>
public sealed record UpdatePlayerCommand : ICommand, IAuditableRequest, IPlayerFields
{
    public Guid Id { get; init; }

    public required string Nickname { get; init; }

    public required string CountryCode { get; init; }

    public PlayerRole Role { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public DateOnly? BirthDate { get; init; }

    public string? Bio { get; init; }

    public FileUpload? Avatar { get; init; }

    public bool RemoveAvatar { get; init; }

    public string AuditEntityType => nameof(Player);

    public string? AuditEntityId => Id.ToString();
}