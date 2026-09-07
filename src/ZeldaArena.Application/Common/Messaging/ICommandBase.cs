namespace ZeldaArena.Application.Common.Messaging;

/// <summary>
/// Общий маркер команд независимо от того, возвращают они значение или нет.
/// Нужен исключительно behaviors: по нему <c>TransactionBehavior</c> отличает команду
/// от запроса и не открывает транзакцию там, где ничего не меняется.
/// </summary>
public interface ICommandBase;