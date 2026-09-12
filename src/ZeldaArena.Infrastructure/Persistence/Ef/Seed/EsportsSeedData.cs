using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

public static class EsportsSeedData
{
    private const int PlayersPerTeam = 5;
    private const int RandomSeed = 20260906;

    private static readonly PlayerRole[] RoleRotation =
    [
        PlayerRole.Attacker,
        PlayerRole.Attacker,
        PlayerRole.Defender,
        PlayerRole.Support,
        PlayerRole.Flex,
    ];

    public static IReadOnlyList<Team> Teams()
    {
        var definitions = new (string Name, string Tag, string Country, Region Region, int Rating)[]
        {
            ("Hyrule Knights", "HYR", "RU", Region.Cis, 1842),
            ("Kakariko Guard", "KAK", "RU", Region.Cis, 1731),
            ("Gerudo Valley", "GRD", "KZ", Region.Cis, 1688),
            ("Zora Domain", "ZOR", "SE", Region.Europe, 1795),
            ("Goron City", "GOR", "DE", Region.Europe, 1654),
            ("Rito Village", "RIT", "FR", Region.Europe, 1723),
            ("Lost Woods", "LWD", "PL", Region.Europe, 1580),
            ("Death Mountain", "DTM", "US", Region.NorthAmerica, 1766),
            ("Lake Hylia", "LKH", "CA", Region.NorthAmerica, 1612),
            ("Faron Grove", "FAR", "BR", Region.SouthAmerica, 1547),
            ("Eldin Peaks", "ELD", "KR", Region.Asia, 1809),
            ("Skyloft", "SKY", "JP", Region.Asia, 1701),
        };

        return [.. definitions.Select(definition => Team.Create(
            Slug.From(definition.Name),
            definition.Name,
            definition.Tag,
            CountryCode.From(definition.Country),
            definition.Region,
            definition.Rating,
            foundedAt: new DateOnly(2019 + (definition.Rating % 5), 1 + (definition.Rating % 12), 15),
            description: $"Команда {definition.Name} выступает в регионе {definition.Region}."))];
    }

    public static IReadOnlyList<Player> PlayersWithRosters(
        IReadOnlyList<Team> teams,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(teams);

        var players = new List<Player>(teams.Count * PlayersPerTeam);

        foreach (var team in teams)
        {
            for (var slot = 0; slot < PlayersPerTeam; slot++)
            {
                var role = RoleRotation[slot];
                var nickname = $"{team.Tag.ToLowerInvariant()}_{role.ToString().ToLowerInvariant()}{slot + 1}";

                var player = Player.Create(
                    Slug.From(nickname),
                    nickname,
                    team.Country,
                    role,
                    firstName: null,
                    lastName: null,
                    birthDate: new DateOnly(1998 + (slot % 6), 3 + slot, 10 + slot),
                    bio: $"Игрок команды {team.Name} на позиции {role}.");

                players.Add(player);
                team.AddPlayer(player.Id, role, now.AddYears(-1).AddDays(slot));
            }
        }

        return players;
    }

    public static IReadOnlyList<Tournament> Tournaments(DateTimeOffset now, IReadOnlyList<Team> teams)
    {
        ArgumentNullException.ThrowIfNull(teams);

        var finished = Tournament.Announce(
            Slug.From("zelda-masters-2025"),
            "Zelda Masters 2025",
            TournamentTier.S,
            Region.Global,
            new Money(3_000_000m, Money.DefaultCurrency),
            now.AddDays(-120),
            now.AddDays(-113),
            "Главный турнир прошлого сезона.",
            "<h3>Регламент</h3><p>Двойная сетка, финал в формате Bo5.</p>");
        AddParticipants(finished, teams, ParticipantSlots()[0]);
        finished.Start();
        finished.Finish();
        finished.SetFeatured(true);

        var ongoing = Tournament.Announce(
            Slug.From("hyrule-open-2026"),
            "Hyrule Open 2026",
            TournamentTier.A,
            Region.Cis,
            new Money(1_200_000m, Money.DefaultCurrency),
            now.AddDays(-2),
            now.AddDays(5),
            "Открытый турнир СНГ, идёт прямо сейчас.",
            "<h3>Регламент</h3><p>Групповой этап Bo1, плей-офф Bo3.</p>");
        AddParticipants(ongoing, teams, ParticipantSlots()[1]);
        ongoing.Start();
        ongoing.SetFeatured(true);

        var soon = Tournament.Announce(
            Slug.From("gerudo-cup-2026"),
            "Gerudo Cup 2026",
            TournamentTier.B,
            Region.Europe,
            new Money(450_000m, Money.DefaultCurrency),
            now.AddDays(10),
            now.AddDays(14),
            "Европейский кубок, старт через полторы недели.",
            "<h3>Регламент</h3><p>Single elimination, все матчи Bo3.</p>");
        AddParticipants(soon, teams, ParticipantSlots()[2]);

        var later = Tournament.Announce(
            Slug.From("skyloft-invitational-2026"),
            "Skyloft Invitational 2026",
            TournamentTier.A,
            Region.Asia,
            new Money(900_000m, Money.DefaultCurrency),
            now.AddDays(30),
            now.AddDays(36),
            "Приглашённые команды Азии и Европы.",
            "<h3>Регламент</h3><p>Круговой этап, финал Bo5.</p>");
        AddParticipants(later, teams, ParticipantSlots()[3]);

        return [finished, ongoing, soon, later];
    }

    private static void AddParticipants(Tournament tournament, IReadOnlyList<Team> teams, int[] slots)
    {
        for (var seed = 0; seed < slots.Length; seed++)
        {
            tournament.AddTeam(teams[slots[seed]].Id, seed + 1);
        }
    }

    public static IReadOnlyList<int[]> ParticipantSlots() =>
    [
        [0, 1, 2, 3, 4, 5],
        [3, 4, 5, 6, 7, 8],
        [6, 7, 8, 9, 10, 11],
        [0, 2, 4, 6, 8, 10],
    ];

    public static IReadOnlyList<Match> Matches(
        DateTimeOffset now,
        IReadOnlyList<Tournament> tournaments,
        IReadOnlyList<Team> teams,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> playersByTeam)
    {
        ArgumentNullException.ThrowIfNull(tournaments);
        ArgumentNullException.ThrowIfNull(teams);
        ArgumentNullException.ThrowIfNull(playersByTeam);

        var random = new Random(RandomSeed);
        var slots = ParticipantSlots();
        var matches = new List<Match>(40);

        // Прошедший турнир: все матчи доиграны.
        matches.AddRange(Build(
            tournaments[0], slots[0], teams, count: 12, startAt: now.AddDays(-119), stepHours: 3,
            finished: 12, live: 0, postponed: 0, canceled: 0, random, playersByTeam));

        // Идущий турнир: часть доиграна, два матча в эфире, остальные впереди.
        matches.AddRange(Build(
            tournaments[1], slots[1], teams, count: 14, startAt: now.AddDays(-2), stepHours: 4,
            finished: 8, live: 2, postponed: 0, canceled: 0, random, playersByTeam));

        // Предстоящие турниры: расписание с одним переносом и одной отменой.
        matches.AddRange(Build(
            tournaments[2], slots[2], teams, count: 8, startAt: now.AddDays(10), stepHours: 5,
            finished: 0, live: 0, postponed: 1, canceled: 0, random, playersByTeam));

        matches.AddRange(Build(
            tournaments[3], slots[3], teams, count: 6, startAt: now.AddDays(30), stepHours: 6,
            finished: 0, live: 0, postponed: 0, canceled: 1, random, playersByTeam));

        return matches;
    }

    private static List<Match> Build(
        Tournament tournament,
        int[] participantSlots,
        IReadOnlyList<Team> teams,
        int count,
        DateTimeOffset startAt,
        int stepHours,
        int finished,
        int live,
        int postponed,
        int canceled,
        Random random,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> playersByTeam)
    {
        var matches = new List<Match>(count);

        for (var index = 0; index < count; index++)
        {
            var teamA = teams[participantSlots[index % participantSlots.Length]];
            var teamB = teams[participantSlots[(index + 1 + (index / participantSlots.Length)) % participantSlots.Length]];

            if (teamA.Id == teamB.Id)
            {
                teamB = teams[participantSlots[(index + 2) % participantSlots.Length]];
            }

            var bestOf = index % 5 == 4 ? 5 : 3;
            var scheduledAt = startAt.AddHours(index * stepHours);

            var match = Match.Schedule(
                tournament.Id,
                teamA.Id,
                teamB.Id,
                scheduledAt,
                bestOf,
                streamUrl: $"https://stream.example.com/{tournament.Slug.Value}/{index + 1}");

            if (index < finished)
            {
                PlayOut(match, teamA, teamB, scheduledAt, random, playersByTeam);
            }
            else if (index < finished + live)
            {
                match.Start(scheduledAt);
                match.UpdateScore(random.Next(0, match.WinsRequired), random.Next(0, match.WinsRequired));
            }
            else if (index < finished + live + postponed)
            {
                match.Postpone(scheduledAt.AddDays(1));
            }
            else if (index < finished + live + postponed + canceled)
            {
                match.Cancel();
            }

            match.ClearDomainEvents();
            matches.Add(match);
        }

        return matches;
    }

    private static void PlayOut(
        Match match,
        Team teamA,
        Team teamB,
        DateTimeOffset scheduledAt,
        Random random,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> playersByTeam)
    {
        match.Start(scheduledAt);

        var winsRequired = match.WinsRequired;
        var loserScore = random.Next(0, winsRequired);
        var teamAWins = random.Next(0, 2) == 0;

        match.UpdateScore(
            teamAWins ? winsRequired : loserScore,
            teamAWins ? loserScore : winsRequired);

        match.Finish(scheduledAt.AddHours(2));

        foreach (var team in new[] { teamA, teamB })
        {
            if (!playersByTeam.TryGetValue(team.Id, out var playerIds))
            {
                continue;
            }

            foreach (var playerId in playerIds)
            {
                match.AddPlayerStats(
                    playerId,
                    team.Id,
                    kills: random.Next(4, 26),
                    deaths: random.Next(3, 22),
                    assists: random.Next(0, 14),
                    damage: random.Next(1200, 4800),
                    rating: Math.Round((decimal)random.NextDouble() + 0.6m, 2));
            }
        }
    }
}