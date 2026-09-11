using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Carts.Commands.AddCartItem;
using ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;
using ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;
using ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;
using ZeldaArena.Application.Features.Carts.Queries.GetCart;
using ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Shop;

/// <summary>
/// Корзина гостя и пользователя (docs/SPEC.md §9.3, п. 13; §14.1). Владелец задаётся
/// только контекстом — вход и проверенная гостевая кука; в командах его нет.
/// </summary>
public class CartScenarioTests
{
    private static readonly Guid Guest = Guid.CreateVersion7();
    private static readonly Guid User = Guid.CreateVersion7();

    private readonly ShopWorld _world = new();
    private readonly Product _keyboard;
    private readonly Product _mouse;
    private readonly Product _lastPieces;

    public CartScenarioTests()
    {
        _keyboard = _world.AddProduct("Hyrule K1", 6990m, stock: 24);
        _mouse = _world.AddProduct("Sheikah S1", 5990m, stock: 40, category: _world.Mice);
        _lastPieces = _world.AddProduct("Gerudo Sand", 11490m, stock: 3);
        _world.BrowseAsGuest(Guest);
    }

    [Fact]
    public async Task Guest_cart_is_created_on_the_first_add()
    {
        _world.Carts.Entities.ShouldBeEmpty();

        var result = await AddAsync(_keyboard.Id, 2);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ItemCount.ShouldBe(2);

        var cart = _world.CartOf(anonymousId: Guest).ShouldNotBeNull();
        cart.UserId.ShouldBeNull();
        cart.Items.ShouldHaveSingleItem().PriceSnapshot.ShouldBe(6990m);
    }

    [Fact]
    public async Task Adding_the_same_product_again_increases_one_line()
    {
        await AddAsync(_keyboard.Id);
        await AddAsync(_keyboard.Id, 2);

        _world.CartOf(anonymousId: Guest)!.Items.ShouldHaveSingleItem().Quantity.ShouldBe(3);
    }

    /// <summary>Остаток проверяется по сумме «в корзине + добавляемое» (§15).</summary>
    [Fact]
    public async Task Stock_is_checked_against_what_is_already_in_the_cart()
    {
        await AddAsync(_lastPieces.Id, 2);

        var result = await AddAsync(_lastPieces.Id, 2);

        result.Error.ShouldBe(ShopErrors.InsufficientStock(3));
        result.Error.Arguments.ShouldBe([3]);
        _world.CartOf(anonymousId: Guest)!.Items.Single().Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task Sold_out_and_withdrawn_products_are_refused_without_an_exception()
    {
        var soldOut = _world.AddProduct("Goron Heavy", 12990m, stock: 0);
        var withdrawn = _world.AddProduct("Retired", 1990m, stock: 5, isActive: false);

        (await AddAsync(soldOut.Id)).Error.ShouldBe(ShopErrors.OutOfStock);
        (await AddAsync(withdrawn.Id)).Error.ShouldBe(ShopErrors.ProductUnavailable);
        (await AddAsync(Guid.CreateVersion7())).Error.ShouldBe(ShopErrors.ProductNotFound);

        _world.Carts.Entities.ShouldBeEmpty();
    }

    /// <summary>Ни входа, ни куки — корзину не к чему привязать; сказать об этом, а не терять товар.</summary>
    [Fact]
    public async Task Without_a_session_the_cart_is_unavailable()
    {
        _world.Guest.AnonymousId = null;

        (await AddAsync(_keyboard.Id)).Error.ShouldBe(ShopErrors.CartUnavailable);
        (await GetCartAsync()).IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public async Task Quantity_is_set_absolutely_and_limited_by_stock()
    {
        await AddAsync(_lastPieces.Id);

        (await ChangeAsync(_lastPieces.Id, 3)).IsSuccess.ShouldBeTrue();
        (await ChangeAsync(_lastPieces.Id, 4)).Error.ShouldBe(ShopErrors.InsufficientStock(3));

        _world.CartOf(anonymousId: Guest)!.Items.Single().Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task Changing_or_removing_a_product_not_in_the_cart_is_refused()
    {
        await AddAsync(_keyboard.Id);

        (await ChangeAsync(_mouse.Id, 2)).Error.ShouldBe(ShopErrors.ItemNotFound);
        (await RemoveAsync(_mouse.Id)).Error.ShouldBe(ShopErrors.ItemNotFound);
    }

    /// <summary>Снятый с продажи товар нельзя купить, но убрать из корзины — обязательно можно.</summary>
    [Fact]
    public async Task A_withdrawn_product_can_still_be_removed()
    {
        await AddAsync(_keyboard.Id);
        _keyboard.Deactivate();

        (await RemoveAsync(_keyboard.Id)).IsSuccess.ShouldBeTrue();
        _world.CartOf(anonymousId: Guest)!.IsEmpty.ShouldBeTrue();
    }

    /// <summary>
    /// IDOR (§15): у команды нет ни идентификатора корзины, ни идентификатора гостя —
    /// другой гость работает со своей корзиной и до чужой не дотягивается.
    /// </summary>
    [Fact]
    public async Task Another_guest_never_sees_or_touches_this_cart()
    {
        await AddAsync(_keyboard.Id, 2);

        _world.BrowseAsGuest(Guid.CreateVersion7());

        (await GetCartAsync()).IsEmpty.ShouldBeTrue();
        (await RemoveAsync(_keyboard.Id)).Error.ShouldBe(ShopErrors.ItemNotFound);
        _world.CartOf(anonymousId: Guest)!.Items.Single().Quantity.ShouldBe(2);
    }

    /// <summary>Итог — из текущих цен на сервере, а не из снапшота и не с клиента (§15, §20 п. 7).</summary>
    [Fact]
    public async Task Cart_totals_use_the_current_server_price()
    {
        await AddAsync(_keyboard.Id, 2);
        await AddAsync(_mouse.Id);
        _keyboard.ChangePrice(new Money(7490m, Money.DefaultCurrency));

        var cart = await GetCartAsync();

        cart.Lines.Select(line => line.Name).ShouldBe(["Hyrule K1", "Sheikah S1"]);
        cart.Lines[0].UnitPrice.ShouldBe(7490m);
        cart.Subtotal.ShouldBe((2 * 7490m) + 5990m);
        cart.ItemCount.ShouldBe(3);
        cart.CanCheckout.ShouldBeTrue();
    }

    /// <summary>Остаток меняется без участия корзины — проблема видна при каждом показе.</summary>
    [Fact]
    public async Task Lines_that_cannot_be_bought_block_checkout()
    {
        await AddAsync(_lastPieces.Id, 3);
        await AddAsync(_mouse.Id);

        _lastPieces.SetStock(1);
        _mouse.Deactivate();

        var cart = await GetCartAsync();

        cart.Lines.Single(line => line.ProductId == _lastPieces.Id).Problem.ShouldBe(CartLineProblem.InsufficientStock);
        cart.Lines.Single(line => line.ProductId == _mouse.Id).Problem.ShouldBe(CartLineProblem.Unavailable);
        cart.CanCheckout.ShouldBeFalse();
    }

    [Fact]
    public async Task Mini_cart_counts_pieces_of_the_current_owner()
    {
        await AddAsync(_keyboard.Id, 2);
        await AddAsync(_mouse.Id);

        (await MiniCartAsync()).ItemCount.ShouldBe(3);

        _world.SignInAs(User);
        (await MiniCartAsync()).ItemCount.ShouldBe(0);
    }

    /// <summary>§19: «корзина гостя сливается с пользовательской при входе».</summary>
    [Fact]
    public async Task Signing_in_merges_the_guest_cart_into_the_user_cart()
    {
        _world.SignInAs(User);
        await AddAsync(_keyboard.Id);
        await AddAsync(_lastPieces.Id, 2);

        _world.BrowseAsGuest(Guest);
        await AddAsync(_keyboard.Id, 2);
        await AddAsync(_lastPieces.Id, 2);
        await AddAsync(_mouse.Id);

        _world.SignInAs(User);
        var result = await MergeAsync();

        result.IsSuccess.ShouldBeTrue();

        var merged = _world.CartOf(userId: User).ShouldNotBeNull();
        merged.Items.Single(item => item.ProductId == _keyboard.Id).Quantity.ShouldBe(3);

        // 2 + 2 при остатке 3 — не больше остатка (правило Cart.MergeFrom).
        merged.Items.Single(item => item.ProductId == _lastPieces.Id).Quantity.ShouldBe(3);
        merged.Items.Single(item => item.ProductId == _mouse.Id).Quantity.ShouldBe(1);
        result.Value.ItemCount.ShouldBe(7);

        // Гостевая корзина удалена: второй раз её содержимое не вольётся.
        _world.CartOf(anonymousId: Guest).ShouldBeNull();
    }

    [Fact]
    public async Task Merging_into_a_user_without_a_cart_creates_one()
    {
        await AddAsync(_mouse.Id, 2);

        _world.SignInAs(User);
        await MergeAsync();

        _world.CartOf(userId: User)!.Items.ShouldHaveSingleItem().Quantity.ShouldBe(2);
        _world.Carts.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Nothing_to_merge_is_not_an_error()
    {
        _world.SignInAs(User);

        (await MergeAsync()).IsSuccess.ShouldBeTrue();
        _world.Carts.Entities.ShouldBeEmpty();
    }

    [Fact]
    public void Validators_reject_quantities_outside_the_line_limit()
    {
        new AddCartItemCommandValidator().Validate(new AddCartItemCommand(_keyboard.Id, 0)).IsValid.ShouldBeFalse();
        new AddCartItemCommandValidator()
            .Validate(new AddCartItemCommand(_keyboard.Id, CartStockCheck.MaxQuantityPerLine + 1)).IsValid.ShouldBeFalse();
        new ChangeCartItemQuantityCommandValidator()
            .Validate(new ChangeCartItemQuantityCommand(_keyboard.Id, -1)).IsValid.ShouldBeFalse();
        new RemoveCartItemCommandValidator().Validate(new RemoveCartItemCommand(Guid.Empty)).IsValid.ShouldBeFalse();
    }

    private Task<Result<CartSummaryDto>> AddAsync(Guid productId, int quantity = 1) =>
        new AddCartItemCommandHandler(_world.Locator(), _world.ReadProducts(), _world.UnitOfWork)
            .Handle(new AddCartItemCommand(productId, quantity), CancellationToken.None);

    private Task<Result<CartSummaryDto>> ChangeAsync(Guid productId, int quantity) =>
        new ChangeCartItemQuantityCommandHandler(_world.Locator(), _world.ReadProducts(), _world.UnitOfWork)
            .Handle(new ChangeCartItemQuantityCommand(productId, quantity), CancellationToken.None);

    private Task<Result<CartSummaryDto>> RemoveAsync(Guid productId) =>
        new RemoveCartItemCommandHandler(_world.Locator(), _world.UnitOfWork)
            .Handle(new RemoveCartItemCommand(productId), CancellationToken.None);

    private Task<Result<CartSummaryDto>> MergeAsync() =>
        new MergeGuestCartCommandHandler(
                _world.CurrentUser,
                _world.Guest,
                _world.Locator(),
                _world.Carts,
                _world.Products,
                _world.UnitOfWork)
            .Handle(new MergeGuestCartCommand(), CancellationToken.None);

    private Task<CartDto> GetCartAsync() =>
        new GetCartQueryHandler(
                _world.Locator(),
                new InMemoryReadRepository<CartItem>(_world.CartItems),
                _world.ReadProducts(),
                new InMemoryQueryExecutor())
            .Handle(new GetCartQuery(), CancellationToken.None);

    private Task<CartSummaryDto> MiniCartAsync() =>
        new GetMiniCartQueryHandler(
                _world.Locator(),
                new InMemoryReadRepository<CartItem>(_world.CartItems),
                new InMemoryQueryExecutor())
            .Handle(new GetMiniCartQuery(), CancellationToken.None);
}