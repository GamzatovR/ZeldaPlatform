namespace ZeldaArena.Application.Common.Models.Identity;

public sealed record RecoveryCodes(IReadOnlyList<string> Codes)
{
    public static readonly RecoveryCodes Empty = new([]);
}