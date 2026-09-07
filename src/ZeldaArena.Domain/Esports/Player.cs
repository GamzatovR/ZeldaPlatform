using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Профессиональный игрок. Существует независимо от команд: состав историчен,
/// поэтому игрок переживает переходы между командами (docs/SPEC.md §6).
/// </summary>
public class Player : BaseEntity
{
    private readonly List<RosterEntry> _rosterEntries = [];

    private Player()
    {
    }

    public Slug Slug { get; private set; } = null!;

    public string Nickname { get; private set; } = null!;

    public string? FirstName { get; private set; }

    public string? LastName { get; private set; }

    public CountryCode Country { get; private set; } = null!;

    public DateOnly? BirthDate { get; private set; }

    public PlayerRole Role { get; private set; }

    public string? AvatarPath { get; private set; }

    public string? Bio { get; private set; }

    public IReadOnlyCollection<RosterEntry> RosterEntries => _rosterEntries.AsReadOnly();

    public static Player Create(
        Slug slug,
        string nickname,
        CountryCode country,
        PlayerRole role,
        string? firstName = null,
        string? lastName = null,
        DateOnly? birthDate = null,
        string? bio = null)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname);

        return new Player
        {
            Slug = slug,
            Nickname = nickname.Trim(),
            Country = country,
            Role = role,
            FirstName = Normalize(firstName),
            LastName = Normalize(lastName),
            BirthDate = birthDate,
            Bio = Normalize(bio),
        };
    }

    public void UpdateProfile(
        string nickname,
        CountryCode country,
        PlayerRole role,
        string? firstName,
        string? lastName,
        DateOnly? birthDate,
        string? bio)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname);

        Nickname = nickname.Trim();
        Country = country;
        Role = role;
        FirstName = Normalize(firstName);
        LastName = Normalize(lastName);
        BirthDate = birthDate;
        Bio = Normalize(bio);
    }

    public void ChangeAvatar(string? avatarPath) => AvatarPath = Normalize(avatarPath);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}