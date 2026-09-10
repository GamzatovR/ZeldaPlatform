namespace ZeldaArena.Web.Constants;

/// <summary>
/// Имена символов спрайта <c>wwwroot/img/icons.svg</c>. Константы, а не строковые
/// литералы по разметке (CLAUDE.md, «Стиль кода»): опечатка в <c>&lt;icon name&gt;</c>
/// не даёт ни ошибки сборки, ни исключения в рантайме — иконка просто не рисуется.
///
/// Соответствие именам классов макета — docs/design/design-system.md §12.
/// Тест <c>IconSpriteTests</c> следит, чтобы каждая константа отсюда была в спрайте,
/// а каждый символ спрайта — здесь.
/// </summary>
public static class IconNames
{
    /// <summary>Путь к спрайту от корня приложения.</summary>
    public const string SpritePath = "/img/icons.svg";

    // Навигация и управление
    public const string Search = "search";
    public const string Menu = "menu";
    public const string Close = "close";
    public const string ChevronDown = "chevron-down";
    public const string ChevronLeft = "chevron-left";
    public const string ChevronRight = "chevron-right";
    public const string ArrowUp = "arrow-up";
    public const string External = "external";

    // Аккаунт и магазин
    public const string User = "user";
    public const string Cart = "cart";
    public const string Bell = "bell";
    public const string Lock = "lock";
    public const string Shield = "shield";
    public const string Trash = "trash";
    public const string Plus = "plus";
    public const string Minus = "minus";

    // Киберспорт
    public const string Trophy = "trophy";
    public const string Users = "users";
    public const string Controller = "controller";
    public const string Play = "play";
    public const string Calendar = "calendar";
    public const string Clock = "clock";
    public const string Globe = "globe";

    // Обратная связь
    public const string Check = "check";
    public const string Alert = "alert";

    // Соцсети
    public const string Facebook = "facebook";
    public const string Twitter = "twitter";
    public const string Youtube = "youtube";
    public const string Twitch = "twitch";
    public const string Instagram = "instagram";
}
