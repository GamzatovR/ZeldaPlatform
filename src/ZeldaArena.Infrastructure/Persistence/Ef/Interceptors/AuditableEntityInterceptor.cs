using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Interceptors;

/// <summary>
/// Проставляет CreatedAt и UpdatedAt у сущностей с <see cref="IAuditableEntity"/>.
///
/// Значения пишутся через метаданные EF, а не через сеттеры: у доменных сущностей
/// публичных сеттеров нет, и появиться они не должны (docs/SPEC.md §5.2, правило 5).
/// Время берётся из TimeProvider, чтобы поведение было проверяемым в тестах;
/// в Фазе 2 сюда придёт порт IDateTimeProvider.
/// </summary>
public sealed class AuditableEntityInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = now;
                    break;

                default:
                    break;
            }
        }
    }
}