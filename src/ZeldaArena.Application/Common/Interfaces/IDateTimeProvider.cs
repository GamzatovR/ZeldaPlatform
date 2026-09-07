namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Единственный источник времени в приложении. Прямые обращения к
/// <c>DateTimeOffset.UtcNow</c> вне этого порта делают непроверяемыми сценарии,
/// которые целиком про время: истечение подписки (docs/SPEC.md §7.5), десятиминутный
/// срок кода подтверждения (§7.6), расписание матчей.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}