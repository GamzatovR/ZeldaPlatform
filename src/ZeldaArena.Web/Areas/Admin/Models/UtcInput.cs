namespace ZeldaArena.Web.Areas.Admin.Models;

/// <summary>
/// Время в формах админки — UTC. Проект хранит время в UTC и так же его показывает
/// («UTC» рядом с временем матча, docs/SPEC.md §9.5): часового пояса зрителя сервер
/// не знает. Поле <c>datetime-local</c> присылает время без зоны, и здесь оно
/// явно объявляется UTC — иначе смещение подставил бы часовой пояс сервера.
/// </summary>
public static class UtcInput
{
    public static DateTimeOffset ToUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), TimeSpan.Zero);

    public static DateTime FromUtc(DateTimeOffset value) =>
        DateTime.SpecifyKind(value.UtcDateTime, DateTimeKind.Unspecified);
}