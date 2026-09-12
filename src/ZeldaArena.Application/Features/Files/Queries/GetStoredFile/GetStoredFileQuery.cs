using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Files.Queries.GetStoredFile;

public sealed record GetStoredFileQuery(string Name) : IQuery<StoredFileContent?>;