using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Extensions;

public static class FormFileExtensions
{
    public static FileUpload? ToFileUpload(this IFormFile? file, Stream? content) =>
        file is null || content is null
            ? null
            : new FileUpload(content, file.FileName, file.ContentType, file.Length);
}