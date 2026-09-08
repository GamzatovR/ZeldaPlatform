namespace ZeldaArena.Web;

/// <summary>
/// Якорь общих ресурсов локализации: <c>IStringLocalizer&lt;SharedResource&gt;</c>
/// ищет по нему файлы <c>Resources/SharedResource.*.resx</c> (docs/SPEC.md §9.5).
///
/// Тип пустой намеренно — он не носитель поведения, а имя, по которому ResourceManager
/// находит нужный файл. Лежит в корневом пространстве имён: путь к ресурсу
/// складывается из ResourcesPath и пространства имён типа, и из
/// <c>Web.Resources.SharedResource</c> получилось бы <c>Resources/Resources/…</c>.
///
/// Нейтральный файл заполнен по-русски, потому что ru — язык по умолчанию.
/// Английский добавляется в Фазе 11 файлом SharedResource.en.resx, без правок кода (EP-7).
/// </summary>
public sealed class SharedResource;