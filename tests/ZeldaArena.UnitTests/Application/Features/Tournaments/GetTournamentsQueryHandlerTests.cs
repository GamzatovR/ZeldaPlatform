using Mapster;

using MapsterMapper;

using ZeldaArena.Application;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Tournaments;

/// <summary>
/// Хендлер проверяется на настоящей коллекции: собранное им выражение исполняется
/// LINQ-провайдером в памяти, поэтому фильтр, сортировка и постраничная выборка
/// проверяются по результату, а не по факту вызова подменённого порта.
/// </summary>
public class GetTournamentsQueryHandlerTests
{
    private static readonly DateTimeOffset Season = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Without_filter_returns_first_page_sorted_by_start_date_descending()
    {
        var result = await Handle(new GetTournamentsQuery());

        result.Page.ShouldBe(1);
        result.TotalCount.ShouldBe(5);
        result.Items.Select(item => item.Name)
            .ShouldBe(["Winter Clash", "Autumn Open", "Summer Major", "Spring Cup", "Asia Invitational"]);
    }

    [Fact]
    public async Task Status_filter_narrows_the_list()
    {
        var result = await Handle(new GetTournamentsQuery { Status = TournamentStatus.Ongoing });

        result.TotalCount.ShouldBe(1);
        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Summer Major");
    }

    [Fact]
    public async Task Region_filter_narrows_the_list()
    {
        var result = await Handle(new GetTournamentsQuery { Region = Region.Asia });

        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Asia Invitational");
    }

    /// <summary>
    /// Граница включающая: турнир, стартующий ровно в указанный день, из списка
    /// не выпадает. Здесь это Summer Major.
    /// </summary>
    [Fact]
    public async Task From_filter_keeps_tournaments_starting_on_that_day_and_later()
    {
        var result = await Handle(new GetTournamentsQuery
        {
            From = new DateOnly(2026, 9, 1),
        });

        result.Items.Select(item => item.Name)
            .ShouldBe(["Winter Clash", "Autumn Open", "Summer Major"]);
    }

    [Fact]
    public async Task From_filter_drops_earlier_tournaments()
    {
        var result = await Handle(new GetTournamentsQuery
        {
            From = new DateOnly(2026, 9, 2),
        });

        result.Items.Select(item => item.Name).ShouldBe(["Winter Clash", "Autumn Open"]);
    }

    /// <summary>«По» включает весь указанный день: Summer Major стартует ровно 1 сентября.</summary>
    [Fact]
    public async Task To_filter_keeps_tournaments_starting_on_that_day_and_earlier()
    {
        var result = await Handle(new GetTournamentsQuery { To = new DateOnly(2026, 9, 1) });

        result.Items.Select(item => item.Name)
            .ShouldBe(["Summer Major", "Spring Cup", "Asia Invitational"]);
    }

    [Fact]
    public async Task Date_range_combines_both_bounds()
    {
        var result = await Handle(new GetTournamentsQuery
        {
            From = new DateOnly(2026, 6, 1),
            To = new DateOnly(2026, 9, 1),
        });

        result.Items.Select(item => item.Name).ShouldBe(["Summer Major", "Spring Cup"]);
    }

    [Fact]
    public async Task Prize_filter_keeps_prize_pools_from_the_given_amount()
    {
        var result = await Handle(new GetTournamentsQuery { PrizeMin = 250_000m });

        result.Items.Select(item => item.Name)
            .ShouldBe(["Winter Clash", "Autumn Open", "Summer Major"]);
    }

    /// <summary>
    /// Перевёрнутый диапазон выбирается обычной формой, поэтому это не ошибка валидации,
    /// а пустой результат: отказ валидатора до Фазы 11 обернулся бы ошибкой сервера.
    /// </summary>
    [Fact]
    public async Task Reversed_date_range_gives_an_empty_list_not_an_error()
    {
        var query = new GetTournamentsQuery { From = new DateOnly(2026, 9, 2), To = new DateOnly(2026, 9, 1) };

        new GetTournamentsQueryValidator().Validate(query).IsValid.ShouldBeTrue();
        (await Handle(query)).TotalCount.ShouldBe(0);
    }

    [Fact]
    public void Validator_rejects_a_negative_prize() =>
        new GetTournamentsQueryValidator()
            .Validate(new GetTournamentsQuery { PrizeMin = -1 })
            .IsValid.ShouldBeFalse();

    [Fact]
    public async Task Search_is_case_insensitive()
    {
        var result = await Handle(new GetTournamentsQuery { Search = "MAJOR" });

        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Summer Major");
    }

    [Fact]
    public async Task Filters_combine()
    {
        var result = await Handle(new GetTournamentsQuery
        {
            Region = Region.Europe,
            Status = TournamentStatus.Announced,
        });

        result.Items.Select(item => item.Name).ShouldBe(["Winter Clash", "Spring Cup"]);
    }

    [Fact]
    public async Task Prize_sort_orders_by_prize_pool()
    {
        var result = await Handle(new GetTournamentsQuery
        {
            Sort = TournamentSorting.PrizeDescending,
        });

        result.Items.First().Name.ShouldBe("Summer Major");
        result.Items.Last().Name.ShouldBe("Spring Cup");
    }

    /// <summary>
    /// Сортировать можно только тем, что перечислено в whitelist. Чужой ключ не роняет
    /// список и не уезжает в запрос — он заменяется сортировкой по умолчанию (§15).
    /// </summary>
    [Fact]
    public async Task Unknown_sort_key_falls_back_to_the_default_order()
    {
        var result = await Handle(new GetTournamentsQuery { Sort = "prize); drop table" });

        result.Items.First().Name.ShouldBe("Winter Clash");
    }

    [Fact]
    public async Task Second_page_contains_the_rest()
    {
        var result = await Handle(new GetTournamentsQuery { Page = 2, PageSize = 12 });

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(5);
        result.TotalPages.ShouldBe(1);
    }

    [Fact]
    public async Task Page_size_outside_the_whitelist_is_normalized()
    {
        var result = await Handle(new GetTournamentsQuery { PageSize = 3 });

        result.PageSize.ShouldBe(12);
        result.Items.Count.ShouldBe(5);
    }

    [Fact]
    public async Task Value_objects_are_projected_into_primitives()
    {
        var result = await Handle(new GetTournamentsQuery { Search = "Summer" });

        var item = result.Items.ShouldHaveSingleItem();
        item.Slug.ShouldBe("summer-major");
        item.PrizePoolAmount.ShouldBe(500_000m);
        item.PrizePoolCurrency.ShouldBe("RUB");
        item.Tier.ShouldBe(TournamentTier.S);
    }

    [Fact]
    public async Task Empty_source_gives_one_empty_page()
    {
        var handler = new GetTournamentsQueryHandler(
            new InMemoryReadRepository<Tournament>([]),
            new InMemoryQueryExecutor(),
            Mapper());

        var result = await handler.Handle(new GetTournamentsQuery(), CancellationToken.None);

        result.Items.ShouldBeEmpty();
        result.TotalPages.ShouldBe(1);
    }

    private static async Task<PagedResult<TournamentListItemDto>> Handle(GetTournamentsQuery query)
    {
        var handler = new GetTournamentsQueryHandler(
            new InMemoryReadRepository<Tournament>(Catalog()),
            new InMemoryQueryExecutor(),
            Mapper());

        return await handler.Handle(query, CancellationToken.None);
    }

    private static IMapper Mapper()
    {
        var configuration = new TypeAdapterConfig();
        configuration.Scan(ApplicationAssemblyReference.Assembly);

        return new MapsterMapper.Mapper(configuration);
    }

    private static IReadOnlyList<Tournament> Catalog()
    {
        var spring = Announce("spring-cup", "Spring Cup", TournamentTier.B, Region.Europe, 100_000m, 0);
        var summer = Announce("summer-major", "Summer Major", TournamentTier.S, Region.Europe, 500_000m, 3);
        var autumn = Announce("autumn-open", "Autumn Open", TournamentTier.A, Region.Cis, 250_000m, 6);
        var winter = Announce("winter-clash", "Winter Clash", TournamentTier.A, Region.Europe, 300_000m, 9);
        var asia = Announce("asia-invitational", "Asia Invitational", TournamentTier.B, Region.Asia, 150_000m, -3);

        summer.Start();

        return [spring, summer, autumn, winter, asia];
    }

    private static Tournament Announce(
        string slug,
        string name,
        TournamentTier tier,
        Region region,
        decimal prize,
        int monthsFromSeason)
    {
        var startsAt = Season.AddMonths(monthsFromSeason);

        return Tournament.Announce(
            Slug.From(slug),
            name,
            tier,
            region,
            Money.FromRubles(prize),
            startsAt,
            startsAt.AddDays(7));
    }
}