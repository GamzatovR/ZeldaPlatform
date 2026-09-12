using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdateFeature;

/// <summary>Название и описание фичи. Код не меняется: по нему код проверяет доступ.</summary>
public sealed record UpdateFeatureCommand(Guid Id, string Name, string? Description)
    : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => Id.ToString();
}