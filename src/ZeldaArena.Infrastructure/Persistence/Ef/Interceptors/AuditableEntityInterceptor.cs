using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Interceptors;

/// <summary>
/// Проставляет CreatedAt и UpdatedAt у сущностей с <see cref="IAuditableEntity"/>.
///
/// Значения пишутся через метаданные EF, а не через сеттеры: у доменных сущностей
/// публичных сеттеров нет, и появиться они не должны (docs/SPEC.md §5.2, правило 5).
/// Время берётся из порта <see cref="IDateTimeProvider"/> — единственного источника
/// времени в приложении, чтобы поведение было проверяемым в тестах.
/// </summary>
public sealed class AuditableEntityInterceptor(IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;

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