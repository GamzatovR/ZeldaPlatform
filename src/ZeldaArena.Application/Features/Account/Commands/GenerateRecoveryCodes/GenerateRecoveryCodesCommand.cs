using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models.Identity;

namespace ZeldaArena.Application.Features.Account.Commands.GenerateRecoveryCodes;

/// <summary>Выпуск нового комплекта кодов восстановления; прежние перестают работать.</summary>
public sealed record GenerateRecoveryCodesCommand : ICommand<RecoveryCodes>, IAuditableRequest
{
    public string AuditEntityType => AccountAudit.EntityType;

    public string? AuditEntityId => null;
}