using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;
using ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Shop;

/// <summary>Каталог тем же механизмом, что турниры и команды.</summary>
public class GetProductsQueryHandlerTests
{
    private readonly ShopWorld _world = new();

    public GetProductsQueryHandlerTests()
    {
        _world.AddProduct("Hyrule K1", 6990m, stock: 24);
        _world.AddProduct("Goron Heavy", 12990m, stock: 0);
        _world.AddProduct("Kakariko KB60", 5490m, stock: 3);
        _world.AddProduct("Sheikah S1", 5990m, stock: 40, category: _world.Mice);
        _world.AddProduct("Retired Mouse", 1990m, stock: 5, category: _world.Mice, isActive: false);
    }

    [Fact]
    public async Task Products_withdrawn_from_sale_are_not_listed()
    {
        var result = await Handle(new GetProductsQuery());

        result.TotalCount.ShouldBe(4);
        result.Items.ShouldNotContain(product => product.Name == "Retired Mouse");
    }

    [Fact]
    public async Task Category_filter_uses_the_slug_from_the_address()
    {
        var result = await Handle(new GetProductsQuery { Category = "mice" });

        result.Items.ShouldHaveSingleItem().Name.ShouldBe("Sheikah S1");
    }

    /// <summary>Испорченная ссылка — пустой список, а не снятый фильтр.</summary>
    [Theory]
    [InlineData("headsets")]
    [InlineData("Не слаг!")]
    public async Task Unknown_category_gives_an_empty_list(string category)
    {
        var result = await Handle(new GetProductsQuery { Category = category });

        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Price_range_is_inclusive()
    {
        var result = await Handle(new GetProductsQuery { PriceMin = 5490m, PriceMax = 6990m, Sort = ProductSorting.PriceAscending });

        result.Items.Select(product => product.Name).ShouldBe(["Kakariko KB60", "Sheikah S1", "Hyrule K1"]);
    }

    /// <summary>Перевёрнутый диапазон — не ошибка, а пустая выборка (урок Фазы 6).</summary>
    [Fact]
    public async Task Reversed_price_range_is_an_empty_result_not_an_error()
    {
        var query = new GetProductsQuery { PriceMin = 10000m, PriceMax = 1000m };

        new GetProductsQueryValidator().Validate(query).IsValid.ShouldBeTrue();
        (await Handle(query)).TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task In_stock_filter_hides_sold_out_products()
    {
        var result = await Handle(new GetProductsQuery { InStock = true });

        result.Items.ShouldNotContain(product => product.Name == "Goron Heavy");
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task Price_sorts_go_both_ways()
    {
        var ascending = await Handle(new GetProductsQuery { Sort = ProductSorting.PriceAscending });
        var descending = await Handle(new GetProductsQuery { Sort = ProductSorting.PriceDescending });

        ascending.Items.First().Name.ShouldBe("Kakariko KB60");
        descending.Items.First().Name.ShouldBe("Goron Heavy");
    }

    /// <summary>Сортировка — только ключи whitelist; неизвестный ключ уходит на умолчание.</summary>
    [Fact]
    public async Task Unknown_sort_key_falls_back_to_the_default()
    {
        var result = await Handle(new GetProductsQuery { Sort = "price; DROP TABLE" });

        result.TotalCount.ShouldBe(4);
    }

    [Fact]
    public async Task Card_carries_the_category_name_and_the_stock_state()
    {
        var result = await Handle(new GetProductsQuery { Category = "keyboards", Sort = ProductSorting.NameAscending });

        var goron = result.Items.Single(product => product.Name == "Goron Heavy");
        goron.CategoryName.ShouldBe("Клавиатуры");
        goron.IsInStock.ShouldBeFalse();

        result.Items.Single(product => product.Name == "Kakariko KB60").IsLowStock.ShouldBeTrue();
        result.Items.Single(product => product.Name == "Hyrule K1").IsLowStock.ShouldBeFalse();
    }

    [Fact]
    public async Task Pages_are_cut_on_the_server()
    {
        var result = await Handle(new GetProductsQuery { PageSize = 12, Page = 2 });

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(4);
    }

    [Fact]
    public void Validator_rejects_a_negative_price() =>
        new GetProductsQueryValidator().Validate(new GetProductsQuery { PriceMin = -1m }).IsValid.ShouldBeFalse();

    [Fact]
    public async Task Showcase_offers_only_what_can_be_bought_now()
    {
        var showcase = await new GetShowcaseProductsQueryHandler(
                _world.ReadProducts(),
                _world.ReadCategories(),
                new InMemoryQueryExecutor())
            .Handle(new GetShowcaseProductsQuery(Count: 10), CancellationToken.None);

        showcase.Count.ShouldBe(3);
        showcase.ShouldAllBe(product => product.IsInStock);
    }

    [Fact]
    public async Task Filter_options_list_categories_by_name()
    {
        var options = await new GetShopFilterOptionsQueryHandler(_world.ReadCategories(), new InMemoryQueryExecutor())
            .Handle(new GetShopFilterOptionsQuery(), CancellationToken.None);

        options.Select(option => option.Slug).ShouldBe(["keyboards", "mice"]);
    }

    private Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery query) =>
        new GetProductsQueryHandler(_world.ReadProducts(), _world.ReadCategories(), new InMemoryQueryExecutor())
            .Handle(query, CancellationToken.None);
}