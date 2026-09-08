using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Account.Commands.UpdateProfile;

/// <summary>Правка профиля: отображаемое имя, страна, язык интерфейса.</summary>
public sealed record UpdateProfileCommand(
    string? DisplayName,
    string? CountryCode,
    string? PreferredCulture)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}