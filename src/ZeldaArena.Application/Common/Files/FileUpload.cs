namespace ZeldaArena.Application.Common.Files;

public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length)
{
    public override string ToString() => $"{FileName} ({ContentType}, {Length} B)";
}