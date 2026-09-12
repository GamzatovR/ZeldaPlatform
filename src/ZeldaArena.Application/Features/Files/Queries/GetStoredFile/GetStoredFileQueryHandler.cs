using MediatR;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

public sealed class GetStoredFileQueryHandler(IFileStorage storage)
    : IRequestHandler<GetStoredFileQuery, StoredFileContent?>
{
    public async Task<StoredFileContent?> Handle(GetStoredFileQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ImageUploadRules.IsStoredName(request.Name)
            || ImageUploadRules.FormatOfStoredName(request.Name) is not { } format)
        {
            return null;
        }

        var content = await storage.OpenAsync(request.Name, cancellationToken).ConfigureAwait(false);

        return content is null
            ? null
            : new StoredFileContent(content, ImageUploadRules.ContentTypeOf(format));
    }
}