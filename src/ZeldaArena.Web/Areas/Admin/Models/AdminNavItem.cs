namespace ZeldaArena.Web.Areas.Admin.Models;

/// <summary>
/// Пункт бокового меню админки. Политика у пункта — та же, что у контроллера раздела:
/// модератор не должен видеть ссылку, которая приведёт его на отказ в доступе.
/// </summary>
public sealed record AdminNavItem(string ResourceKey, string Controller, string Icon, string Policy);