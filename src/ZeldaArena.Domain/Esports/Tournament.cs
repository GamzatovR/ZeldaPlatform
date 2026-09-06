using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Esports;

/// <summary>
/// Турнир: набор участников и матчей с общим призовым фондом.
/// Удаление турнира запрещено на уровне схемы (Restrict), иначе каскад стёр бы историю матчей.
/// </summary>
public class Tournament : BaseEntity, IAuditableEntity
{
    private readonly List<TournamentTeam> _participants = [];
    private readonly List<Match> _matches = [];

    private Tournament()
    {
    }

    public Slug Slug { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public TournamentTier Tier { get; private set; }

    public Region Region { get; private set; }

    public Money PrizePool { get; private set; } = null!;

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public TournamentStatus Status { get; private set; }

    /// <summary>
    /// Регламент в HTML. Выводится через Html.Raw, поэтому очищается HtmlSanitizer
    /// на входе, в хендлере команды — не при выводе (docs/SPEC.md §15).
    /// </summary>
    public string? RulesHtml { get; private set; }

    public string? LogoPath { get; private set; }

    public string? BannerPath { get; private set; }

    /// <summary>Показывать ли турнир в блоке избранного на главной.</summary>
    public bool IsFeatured { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<TournamentTeam> Participants => _participants.AsReadOnly();

    public IReadOnlyCollection<Match> Matches => _matches.AsReadOnly();

    public static Tournament Announce(
        Slug slug,
        string name,
        TournamentTier tier,
        Region region,
        Money prizePool,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string? description = null,
        string? rulesHtml = null)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(prizePool);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        InvariantViolationException.ThrowIf(
            endsAt < startsAt,
            "tournament.ends_before_starts",
            "Турнир не может закончиться раньше, чем начнётся.");

        return new Tournament
        {
            Slug = slug,
            Name = name.Trim(),
            Tier = tier,
            Region = region,
            PrizePool = prizePool,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = TournamentStatus.Announced,
            Description = Normalize(description),
            RulesHtml = Normalize(rulesHtml),
        };
    }

    public void UpdateDetails(
        string name,
        TournamentTier tier,
        Region region,
        Money prizePool,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string? description,
        string? rulesHtml)
    {
        ArgumentNullException.ThrowIfNull(prizePool);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        InvariantViolationException.ThrowIf(
            endsAt < startsAt,
            "tournament.ends_before_starts",
            "Турнир не может закончиться раньше, чем начнётся.");

        Name = name.Trim();
        Tier = tier;
        Region = region;
        PrizePool = prizePool;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Description = Normalize(description);
        RulesHtml = Normalize(rulesHtml);
    }

    public void Start()
    {
        InvariantViolationException.ThrowIf(
            Status != TournamentStatus.Announced,
            "tournament.cannot_start",
            $"Начать можно только анонсированный турнир, текущий статус — {Status}.");

        Status = TournamentStatus.Ongoing;
    }

    public void Finish()
    {
        InvariantViolationException.ThrowIf(
            Status != TournamentStatus.Ongoing,
            "tournament.cannot_finish",
            $"Завершить можно только идущий турнир, текущий статус — {Status}.");

        Status = TournamentStatus.Finished;
    }

    public void Cancel()
    {
        InvariantViolationException.ThrowIf(
            Status == TournamentStatus.Finished,
            "tournament.finished_is_read_only",
            "Завершённый турнир отменить нельзя.");

        Status = TournamentStatus.Canceled;
    }

    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;

    public void ChangeImages(string? logoPath, string? bannerPath)
    {
        LogoPath = Normalize(logoPath);
        BannerPath = Normalize(bannerPath);
    }

    public TournamentTeam AddTeam(Guid teamId, int seed)
    {
        InvariantViolationException.ThrowIf(
            Status is TournamentStatus.Finished or TournamentStatus.Canceled,
            "tournament.roster_is_closed",
            "Состав участников закрыт: турнир уже завершён или отменён.");

        InvariantViolationException.ThrowIf(
            _participants.Any(participant => participant.TeamId == teamId),
            "tournament.team_already_participates",
            "Команда уже участвует в этом турнире.");

        var participant = TournamentTeam.Create(Id, teamId, seed);
        _participants.Add(participant);
        return participant;
    }

    public void RemoveTeam(Guid teamId)
    {
        var participant = _participants.SingleOrDefault(item => item.TeamId == teamId);

        InvariantViolationException.ThrowIf(
            participant is null,
            "tournament.team_not_found",
            "Команда не участвует в этом турнире.");

        _participants.Remove(participant!);
    }

    /// <summary>Итоговое место команды. Проставляется, когда турнир доигран.</summary>
    public void SetPlacement(Guid teamId, int placement)
    {
        var participant = _participants.SingleOrDefault(item => item.TeamId == teamId);

        InvariantViolationException.ThrowIf(
            participant is null,
            "tournament.team_not_found",
            "Команда не участвует в этом турнире.");

        participant!.SetPlacement(placement);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
