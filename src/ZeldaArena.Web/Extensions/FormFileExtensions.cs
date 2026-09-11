using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// <c>IFormFile</c> — тип ASP.NET Core, и в команду сценария он не попадает:
/// Application от ASP.NET Core не зависит (docs/SPEC.md §5.2). Поток открывает
/// и закрывает контроллер, команда его только читает.
/// </summary>
public static class FormFileExtensions
{
    public static FileUpload? ToFileUpload(this IFormFile? file, Stream? content) =>
        file is null || content is null
            ? null
            : new FileUpload(content, file.FileName, file.ContentType, file.Length);
}