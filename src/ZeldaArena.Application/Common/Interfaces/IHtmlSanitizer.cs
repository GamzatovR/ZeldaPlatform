namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Очистка HTML на входе (docs/SPEC.md §15): регламент турнира и тело новости выводятся
/// через <c>Html.Raw</c>, и это допустимо только потому, что в базу они попадают уже
/// очищенными. Очищать при выводе поздно — тогда безопасность страницы зависит от того,
/// не забыл ли кто-то вызов во вьюхе.
///
/// Реализация — Ganss.Xss в Infrastructure; Application о библиотеке не знает.
/// </summary>
public interface IHtmlSanitizer
{
    string Sanitize(string html);
}