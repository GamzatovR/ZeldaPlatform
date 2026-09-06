using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Киберспортивная команда. <see cref="OwnerUserId"/> заполнен, если команду создал
/// подписчик с фичей <c>team.create</c>; у команд из сида владельца нет (docs/SPEC.md §4).
/// </summary>
public class Team : BaseEntity, IAuditableEntity
{
    public const int MinRating = 0;

    private readonly List<RosterEntry> _rosterEntries = [];

    private Team()
    {
    }

    public Slug Slug { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    /// <summary>Короткий тег для табло: «HYR», «KAK».</summary>
    public string Tag { get; private set; } = null!;

    public string? LogoPath { get; private set; }

    public CountryCode Country { get; private set; } = null!;

    public Region Region { get; private set; }

    public DateOnly? FoundedAt { get; private set; }

    public int Rating { get; private set; }

    public string? Description { get; private set; }

    /// <summary>Владелец-подписчик или <c>null</c> у команд, заведённых администратором.</summary>
    public Guid? OwnerUserId { get; private set; }

    /// <summary>Команда пользователя показывается публично только после одобрения модератором.</summary>
    public bool IsApproved { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<RosterEntry> RosterEntries => _rosterEntries.AsReadOnly();

    public IEnumerable<RosterEntry> ActiveRoster => _rosterEntries.Where(entry => entry.IsActive);

    /// <summary>
    /// Команда, заведённая администратором или сидом: сразу одобрена, владельца нет.
    /// </summary>
    public static Team Create(
        Slug slug,
        string name,
        string tag,
        CountryCode country,
        Region region,
        int rating = MinRating,
        DateOnly? foundedAt = null,
        string? description = null) =>
        CreateInternal(slug, name, tag, country, region, rating, foundedAt, description, null, true);

    /// <summary>
    /// Команда, созданная подписчиком (фича <c>team.create</c>). Наличие фичи проверяет
    /// хендлер через IEntitlementService — сущность о подписках не знает (docs/SPEC.md §8.1).
    /// Публикуется только после одобрения модератором.
    /// </summary>
    public static Team CreateByUser(
        Guid ownerUserId,
        Slug slug,
        string name,
        string tag,
        CountryCode country,
        Region region,
        DateOnly? foundedAt = null,
        string? description = null)
    {
        InvariantViolationException.ThrowIf(
            ownerUserId == Guid.Empty,
            "team.owner_required",
            "У команды, созданной пользователем, обязан быть владелец.");

        return CreateInternal(slug, name, tag, country, region, MinRating, foundedAt, description, ownerUserId, false);
    }

    public void UpdateProfile(
        string name,
        string tag,
        CountryCode country,
        Region region,
        DateOnly? foundedAt,
        string? description)
    {
        ArgumentNullException.ThrowIfNull(country);

        Name = RequireName(name);
        Tag = RequireTag(tag);
        Country = country;
        Region = region;
        FoundedAt = foundedAt;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void ChangeLogo(string? logoPath) =>
        LogoPath = string.IsNullOrWhiteSpace(logoPath) ? null : logoPath.Trim();

    public void Approve() => IsApproved = true;

    public void Revoke() => IsApproved = false;

    public void UpdateRating(int rating)
    {
        InvariantViolationException.ThrowIf(
            rating < MinRating,
            "team.negative_rating",
            $"Рейтинг команды не может быть меньше {MinRating}.");

        Rating = rating;
    }

    /// <summary>
    /// Добавляет игрока в состав. Внутри команды у игрока не может быть двух открытых записей.
    /// Запрет состоять сразу в двух разных командах — правило между агрегатами: его проверяет
    /// валидатор сценария, потому что сущность не видит чужие составы (docs/SPEC.md §15).
    /// </summary>
    public RosterEntry AddPlayer(Guid playerId, PlayerRole role, DateTimeOffset joinedAt)
    {
        InvariantViolationException.ThrowIf(
            _rosterEntries.Any(entry => entry.PlayerId == playerId && entry.IsActive),
            "team.player_already_in_roster",
            "Игрок уже состоит в этой команде.");

        var entry = RosterEntry.Open(Id, playerId, role, joinedAt);
        _rosterEntries.Add(entry);
        return entry;
    }

    public void RemovePlayer(Guid playerId, DateTimeOffset leftAt)
    {
        var entry = _rosterEntries.SingleOrDefault(item => item.PlayerId == playerId && item.IsActive);

        InvariantViolationException.ThrowIf(
            entry is null,
            "team.player_not_in_roster",
            "Игрока нет в текущем составе команды.");

        entry!.Close(leftAt);
    }

    public void ChangePlayerRole(Guid playerId, PlayerRole role)
    {
        var entry = _rosterEntries.SingleOrDefault(item => item.PlayerId == playerId && item.IsActive);

        InvariantViolationException.ThrowIf(
            entry is null,
            "team.player_not_in_roster",
            "Игрока нет в текущем составе команды.");

        entry!.ChangeRole(role);
    }

    private static Team CreateInternal(
        Slug slug,
        string name,
        string tag,
        CountryCode country,
        Region region,
        int rating,
        DateOnly? foundedAt,
        string? description,
        Guid? ownerUserId,
        bool isApproved)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(country);

        InvariantViolationException.ThrowIf(
            rating < MinRating,
            "team.negative_rating",
            $"Рейтинг команды не может быть меньше {MinRating}.");

        return new Team
        {
            Slug = slug,
            Name = RequireName(name),
            Tag = RequireTag(tag),
            Country = country,
            Region = region,
            Rating = rating,
            FoundedAt = foundedAt,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OwnerUserId = ownerUserId,
            IsApproved = isApproved,
        };
    }

    private static string RequireName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }

    private static string RequireTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var normalized = tag.Trim().ToUpperInvariant();

        InvariantViolationException.ThrowIf(
            normalized.Length is < 2 or > 8,
            "team.invalid_tag",
            $"Тег команды — от 2 до 8 символов, получено «{tag}».");

        return normalized;
    }
}
