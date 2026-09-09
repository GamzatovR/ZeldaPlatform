using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Billing;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;

/// <summary>
/// Заводит платную функцию (docs/SPEC.md §5.4, EP-3). Новая функция — это строка
/// в таблице Features плюс атрибут <c>[RequireFeature]</c> на действии; политика
/// для неё собирается на лету, править список политик не нужно.
/// </summary>
public sealed record CreateFeatureCommand(string Code, string Name, string? Description)
    : ICommand<Guid>, IAuditableRequest
{
    public string AuditEntityType => BillingAudit.Feature;

    public string? AuditEntityId => null;
}