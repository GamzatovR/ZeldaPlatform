namespace ZeldaArena.Web.Models.Layout;

/// <summary>
/// Шапка внутренней страницы. Отдельная модель, а не <c>ViewData</c>:
/// в шапке два поля, и передавать их нетипизированным словарём — ровно тот
/// случай, который запрещает docs/SPEC.md §2 («ViewBag для бизнес-данных»).
/// </summary>
/// <param name="Title">Заголовок. Дублируется в атрибут title ради glitch-эффекта макета.</param>
/// <param name="SubTitle">Надзаголовок акцентным цветом; необязателен.</param>
/// <param name="Compact">
/// Уменьшенный кегль для имён собственных: «Hyrule Knights vs Kakariko Guard» набором
/// в 130px не помещается и на широком экране.
/// </param>
public sealed record PageTitleViewModel(string Title, string? SubTitle = null, bool Compact = false);