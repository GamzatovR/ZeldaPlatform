namespace ZeldaArena.Web.Models.Layout;

public sealed record PageTitleViewModel(string Title, string? SubTitle = null, bool Compact = false);