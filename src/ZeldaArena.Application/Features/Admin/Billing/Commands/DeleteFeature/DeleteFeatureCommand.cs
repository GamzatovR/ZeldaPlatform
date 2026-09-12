using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;

/// <summary>
/// Удаление платной функции. Функция, на которую ссылается код (<c>FeatureCodes</c>),
/// не удаляется: <c>[RequireFeature]</c> перестал бы пускать кого бы то ни было,
/// и починить это можно было бы только правкой кода (docs/adr/ADR-0010).
/// </summary>
public sealed record DeleteFeatureCommand(Guid Id) : ICommand, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => Id.ToString();
}