using MediatR;

namespace ZeldaArena.Application.Common.Messaging;

/// <summary>
/// Запрос на чтение. В отличие от команды возвращает данные напрямую, без обёртки
/// <c>Result</c>: у чтения нет ожидаемых бизнес-неудач, пустой список — это тоже ответ.
/// Транзакция для запросов не открывается.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>;