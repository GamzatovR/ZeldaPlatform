using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Маленький киберспортивный мир для тестов сценариев: команды, игроки, составы,
/// матчи и статистика, собранные настоящими доменными методами. Репозитории отдают
/// одни и те же объекты, поэтому явные соединения в хендлерах (состав ↔ игрок,
/// статистика ↔ игрок) исполняются в памяти так же, как в SQL.
///
/// Навигационные свойства здесь пустые — их заполняет EF Core; проекции обязаны
/// это переживать, и тесты это заодно проверяют.
/// </summary>
internal sealed class EsportsWorld
{
    public static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    public Tournament Tournament { get; } = Tournament.Announce(
        Slug.From("hyrule-open"),
        "Hyrule Open",
        TournamentTier.A,
        Region.Europe,
        Money.FromRubles(100_000m),
        Now.AddDays(-30),
        Now.AddDays(30));

    public List<Team> Teams { get; } = [];

    public List<Player> Players { get; } = [];

    public List<Match> Matches { get; } = [];

    public IEnumerable<RosterEntry> RosterEntries => Teams.SelectMany(team => team.RosterEntries);

    public IEnumerable<PlayerMatchStats> Stats => Matches.SelectMany(match => match.PlayerStats);

    public Team AddTeam(string name, string tag, Region region = Region.Europe, int rating = 1000)
    {
        var team = Team.Create(Slug.From(name), name, tag, CountryCode.From("RU"), region, rating);
        Teams.Add(team);

        return team;
    }

    public Team AddUserTeam(Guid ownerId, string name, string tag)
    {
        var team = Team.CreateByUser(ownerId, Slug.From(name), name, tag, CountryCode.From("RU"), Region.Europe);
        Teams.Add(team);

        return team;
    }

    public Player AddPlayer(string nickname, PlayerRole role = PlayerRole.Attacker, string country = "RU")
    {
        var player = Player.Create(Slug.From(nickname), nickname, CountryCode.From(country), role);
        Players.Add(player);

        return player;
    }

    /// <summary>Игрок в составе команды с указанной даты.</summary>
    public Player Sign(Team team, string nickname, DateTimeOffset joinedAt, PlayerRole role = PlayerRole.Attacker)
    {
        var player = AddPlayer(nickname, role);
        team.AddPlayer(player.Id, role, joinedAt);

        return player;
    }

    public Match Schedule(Team teamA, Team teamB, DateTimeOffset at, int bestOf = 3)
    {
        var match = Match.Schedule(Tournament.Id, teamA.Id, teamB.Id, at, bestOf);
        Matches.Add(match);

        return match;
    }

    /// <summary>Сыгранный матч Bo3 с указанным счётом; счёт обязан определять победителя.</summary>
    public Match Play(Team teamA, Team teamB, DateTimeOffset at, int scoreA, int scoreB)
    {
        var match = Schedule(teamA, teamB, at);
        match.Start(at);
        match.UpdateScore(scoreA, scoreB);
        match.Finish(at.AddHours(2));

        return match;
    }

    public InMemoryReadRepository<T> Read<T>(IEnumerable<T> source)
        where T : BaseEntity => new(source);
}