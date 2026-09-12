using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Application.Common.Rules;

/// <summary>
/// Нарушение инварианта сущности — отказ сценария, а не ошибка сервера.
///
/// В админке правило домена нарушается обычными кнопками, без подделки запроса: вкладка,
/// открытая до того, как коллега завершил матч, двойной клик по «Начать». До Фазы 11
/// необработанное исключение отвечает 500 (docs/PROGRESS.md, Фазы 6–8), а пользователь
/// должен увидеть объяснение. Сообщение не дублируется в хендлере: код и текст берутся
/// из самого исключения, и правило остаётся в одном месте — в сущности.
///
/// Сущность проверяет условие до изменения состояния, поэтому после отказа она та же,
/// что была, а хендлер просто не сохраняет её.
/// </summary>
public static class DomainRules
{
    public static Result Apply(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            operation();

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(new Error(exception.Code, exception.Message));
        }
    }
}