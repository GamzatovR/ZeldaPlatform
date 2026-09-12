namespace ZeldaArena.Application.Features.Account;

/// <summary>
/// Общие значения для записей аудита по сценариям аккаунта (docs/SPEC.md §13).
///
/// Тип сущности задан константой, а не литералом в каждой команде: по нему в админке
/// фильтруется журнал, и достаточно одной опечатки, чтобы часть записей выпала
/// из выборки (docs/CONVENTIONS.md, «никаких магических строк»).
/// </summary>
public static class AccountAudit
{
    /// <summary>
    /// Пользователь живёт в Identity, а не в Domain, поэтому имени типа для nameof нет.
    /// </summary>
    public const string EntityType = "User";
}