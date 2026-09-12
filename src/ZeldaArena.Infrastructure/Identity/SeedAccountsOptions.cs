namespace ZeldaArena.Infrastructure.Identity;

/// <summary>Учётные записи для сида.</summary>
public sealed class SeedAccountsOptions
{
    public const string SectionName = "SeedAccounts";

    public SeedAccountOptions Admin { get; set; } = new();

    public SeedAccountOptions Moderator { get; set; } = new();

    public IList<SeedAccountOptions> Demo { get; set; } = [];
}