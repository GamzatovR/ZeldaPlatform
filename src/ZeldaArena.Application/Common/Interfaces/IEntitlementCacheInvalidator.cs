namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Сброс кэша прав. Отдельный порт, а не метод <see cref="IEntitlementService"/>:
/// сигнатура последнего зафиксирована docs/SPEC.md §7.3 дословно, а инвалидация —
/// другая забота и другой потребитель (§5.5, ISP). Читают права хендлеры и разметка,
/// сбрасывают кэш — обработчики событий подписки и админка тарифов.
///
/// Реализация в Infrastructure держит кэш пять минут (§7.3). Пять минут задержки
/// терпимы при истечении подписки, но недопустимы сразу после оплаты и после того,
/// как администратор снял фичу с тарифа: в обоих случаях пользователь смотрит
/// на результат прямо сейчас.
/// </summary>
public interface IEntitlementCacheInvalidator
{
    /// <summary>Права одного пользователя изменились: оплата, истечение, отзыв подписки.</summary>
    Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Изменился состав фич тарифа или сама фича — задеты все, кто на этом тарифе.
    /// Кто именно, мы не знаем и выяснять не будем: EP-4 демонстрируется вживую,
    /// и доступ обязан измениться у всех сразу.
    /// </summary>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}