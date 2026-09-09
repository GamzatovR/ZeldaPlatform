using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

/// <summary>
/// Наполняет базу демонстрационными данными. Идемпотентен: каждый набор ищется
/// по естественному ключу (код тарифа, слаг, артикул), добавляется только недостающее,
/// повторный запуск ничего не меняет.
///
/// Пользователей и роли заводит IdentitySeeder, он отрабатывает раньше
/// (docs/adr/ADR-0006). Новостям нужен автор с внешним ключом на AspNetUsers,
/// поэтому они сеются здесь, после него. Подписки — тоже: демо-пользователей
/// заводит IdentitySeeder. Заказы — Фаза 7.
/// </summary>
public sealed class DatabaseSeeder(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        await SeedBillingAsync(cancellationToken);
        await SeedCatalogAsync(cancellationToken);
        await SeedEsportsAsync(now, cancellationToken);
        await SeedNewsAsync(now, cancellationToken);
        await SeedSubscriptionsAsync(now, cancellationToken);
    }

    /// <summary>
    /// Новости сеются последними: автором становится первый заведённый пользователь,
    /// то есть администратор из IdentitySeeder. Если учётных записей нет вовсе —
    /// такое бывает на боевом сервере, где сид аккаунтов выключен, — новости
    /// пропускаются, а не падают на внешнем ключе.
    /// </summary>
    private async Task SeedNewsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (await context.NewsArticles.AnyAsync(cancellationToken))
        {
            return;
        }

        var authorId = await context.Users
            .OrderBy(user => user.CreatedAt)
            .Select(user => user.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (authorId == Guid.Empty)
        {
            logger.LogInformation("Сид новостей пропущен: в базе нет ни одного пользователя.");

            return;
        }

        var articles = NewsSeedData.Articles(authorId, now);

        context.NewsArticles.AddRange(articles);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Сид новостей: добавлено {NewsCount}, из них опубликовано {PublishedCount}.",
            articles.Count,
            articles.Count(article => article.IsPublished));
    }

    /// <summary>
    /// Раздаёт подписки демо-пользователям: активная, истёкшая и никакой
    /// (docs/SPEC.md §6). Три состояния нужны, чтобы фича-гейт было видно вживую:
    /// у первого платные функции открыты, у второго закрыты, третий их и не покупал.
    ///
    /// Идемпотентно: подписка ищется по пользователю, повторный запуск ничего
    /// не добавляет и не продлевает уже выданное.
    /// </summary>
    private async Task SeedSubscriptionsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var plan = await context.Plans
            .FirstOrDefaultAsync(item => item.Code == PlanCodes.ProMonth, cancellationToken);

        if (plan is null)
        {
            return;
        }

        // Учётные записи заводит IdentitySeeder по конфигурации, и на боевом сервере
        // его может не быть вовсе — тогда сеять подписки некому.
        var demoUserIds = await context.Users
            .Where(user => SubscriptionSeedData.DemoEmails.Contains(user.Email!))
            .ToDictionaryAsync(user => user.Email!, user => user.Id, cancellationToken);

        var alreadyHaveSubscription = await context.Subscriptions
            .Select(subscription => subscription.UserId)
            .ToListAsync(cancellationToken);

        var added = 0;

        foreach (var (email, startedDaysAgo) in SubscriptionSeedData.Subscriptions)
        {
            if (!demoUserIds.TryGetValue(email, out var userId)
                || alreadyHaveSubscription.Contains(userId))
            {
                continue;
            }

            var subscription = Subscription.Activate(userId, plan, now.AddDays(-startedDaysAgo));

            // Истёкшую подписку домен закрывает своим же методом, а не подстановкой
            // статуса: так у неё появится и событие, и корректная дата.
            if (!subscription.IsActiveAt(now))
            {
                subscription.Expire(now);
            }

            context.Subscriptions.Add(subscription);
            added++;
        }

        if (added == 0)
        {
            return;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Сид подписок: добавлено {Count}.", added);
    }

    private async Task SeedBillingAsync(CancellationToken cancellationToken)
    {
        var existingFeatureCodes = await context.Features
            .Select(feature => feature.Code)
            .ToListAsync(cancellationToken);

        var newFeatures = BillingSeedData.Features()
            .Where(feature => !existingFeatureCodes.Contains(feature.Code))
            .ToList();

        context.Features.AddRange(newFeatures);

        var existingPlanCodes = await context.Plans
            .Select(plan => plan.Code)
            .ToListAsync(cancellationToken);

        var newPlans = BillingSeedData.Plans()
            .Where(plan => !existingPlanCodes.Contains(plan.Code))
            .ToList();

        context.Plans.AddRange(newPlans);

        await context.SaveChangesAsync(cancellationToken);

        await LinkPlanFeaturesAsync(cancellationToken);

        if (newFeatures.Count > 0 || newPlans.Count > 0)
        {
            logger.LogInformation(
                "Сид биллинга: добавлено фич {FeatureCount}, тарифов {PlanCount}.",
                newFeatures.Count,
                newPlans.Count);
        }
    }

    private async Task LinkPlanFeaturesAsync(CancellationToken cancellationToken)
    {
        var featureIdsByCode = await context.Features
            .ToDictionaryAsync(feature => feature.Code, feature => feature.Id, cancellationToken);

        var plans = await context.Plans
            .Include(plan => plan.PlanFeatures)
            .ToListAsync(cancellationToken);

        var links = BillingSeedData.PlanFeatures();

        foreach (var plan in plans)
        {
            if (!links.TryGetValue(plan.Code, out var featureCodes))
            {
                continue;
            }

            foreach (var featureCode in featureCodes)
            {
                if (featureIdsByCode.TryGetValue(featureCode, out var featureId))
                {
                    plan.GrantFeature(featureId);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        var existingCategorySlugs = await context.ProductCategories
            .Select(category => category.Slug)
            .ToListAsync(cancellationToken);

        var newCategories = CatalogSeedData.Categories()
            .Where(category => !existingCategorySlugs.Contains(category.Slug))
            .ToList();

        context.ProductCategories.AddRange(newCategories);
        await context.SaveChangesAsync(cancellationToken);

        var categoryIdsBySlug = await context.ProductCategories
            .ToDictionaryAsync(category => category.Slug.Value, category => category.Id, cancellationToken);

        var existingSkus = await context.Products
            .Select(product => product.Sku)
            .ToListAsync(cancellationToken);

        var newProducts = CatalogSeedData.Products(categoryIdsBySlug)
            .Where(product => !existingSkus.Contains(product.Sku))
            .ToList();

        context.Products.AddRange(newProducts);
        await context.SaveChangesAsync(cancellationToken);

        if (newProducts.Count > 0)
        {
            logger.LogInformation(
                "Сид каталога: добавлено категорий {CategoryCount}, товаров {ProductCount}.",
                newCategories.Count,
                newProducts.Count);
        }
    }

    private async Task SeedEsportsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var teams = await EnsureTeamsAsync(now, cancellationToken);
        var tournaments = await EnsureTournamentsAsync(now, teams, cancellationToken);

        await EnsureMatchesAsync(now, tournaments, teams, cancellationToken);
    }

    private async Task<List<Team>> EnsureTeamsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var existingSlugs = await context.Teams
            .Select(team => team.Slug)
            .ToListAsync(cancellationToken);

        var newTeams = EsportsSeedData.Teams()
            .Where(team => !existingSlugs.Contains(team.Slug))
            .ToList();

        if (newTeams.Count > 0)
        {
            // Игроки и записи состава заводятся вместе с командой: состав историчен,
            // и без него команда была бы пустой карточкой (docs/SPEC.md §6).
            var players = EsportsSeedData.PlayersWithRosters(newTeams, now);

            context.Teams.AddRange(newTeams);
            context.Players.AddRange(players);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Сид киберспорта: добавлено команд {TeamCount}, игроков {PlayerCount}.",
                newTeams.Count,
                players.Count);
        }

        return await context.Teams
            .Include(team => team.RosterEntries)
            .OrderBy(team => team.Slug)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Tournament>> EnsureTournamentsAsync(
        DateTimeOffset now,
        IReadOnlyList<Team> teams,
        CancellationToken cancellationToken)
    {
        var existingSlugs = await context.Tournaments
            .Select(tournament => tournament.Slug)
            .ToListAsync(cancellationToken);

        // Турниры приходят из сида уже с составом участников и в нужном статусе.
        var newTournaments = EsportsSeedData.Tournaments(now, teams)
            .Where(tournament => !existingSlugs.Contains(tournament.Slug))
            .ToList();

        context.Tournaments.AddRange(newTournaments);

        await context.SaveChangesAsync(cancellationToken);

        return await context.Tournaments
            .OrderBy(tournament => tournament.StartsAt)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureMatchesAsync(
        DateTimeOffset now,
        IReadOnlyList<Tournament> tournaments,
        IReadOnlyList<Team> teams,
        CancellationToken cancellationToken)
    {
        if (await context.Matches.AnyAsync(cancellationToken))
        {
            return;
        }

        // Турниры уже отсортированы по дате старта, а это и есть порядок сида:
        // прошедший, идущий и два предстоящих.
        var playersByTeam = teams.ToDictionary(
            team => team.Id,
            team => (IReadOnlyList<Guid>)[.. team.RosterEntries
                .Where(entry => entry.IsActive)
                .Select(entry => entry.PlayerId)]);

        var matches = EsportsSeedData.Matches(now, tournaments, teams, playersByTeam);

        context.Matches.AddRange(matches);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Сид киберспорта: добавлено матчей {MatchCount}, из них Live {LiveCount}.",
            matches.Count,
            matches.Count(match => match.Status == MatchStatus.Live));
    }
}