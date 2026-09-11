using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using ZeldaArena.Application.Features.Carts.Queries.GetCart;
using ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;
using ZeldaArena.Web.Models.Billing;

namespace ZeldaArena.Web.Models.Checkout;

/// <summary>
/// Оформление заказа (docs/SPEC.md §9.3, п. 14): адрес доставки, реквизиты карты
/// и состав корзины для сверки. Ограничения полей повторяют
/// <see cref="PlaceOrderCommandValidator"/> — иначе поле, отвергнутое FluentValidation,
/// до Фазы 11 превращалось бы в ошибку сервера (урок Фазы 6).
///
/// Состав корзины из формы не принимается: он только показывается, а в заказ
/// попадает то, что лежит в корзине на сервере (§15).
/// </summary>
public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "Укажите получателя.")]
    [StringLength(PlaceOrderCommandValidator.MaxRecipientLength, ErrorMessage = "Не длиннее {1} символов.")]
    [Display(Name = "Получатель")]
    public string Recipient { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите телефон.")]
    [StringLength(
        PlaceOrderCommandValidator.MaxPhoneLength,
        MinimumLength = PlaceOrderCommandValidator.MinPhoneLength,
        ErrorMessage = "Телефон — от {2} до {1} символов.")]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите страну.")]
    [StringLength(PlaceOrderCommandValidator.MaxCountryLength, ErrorMessage = "Не длиннее {1} символов.")]
    [Display(Name = "Страна")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите город.")]
    [StringLength(PlaceOrderCommandValidator.MaxCityLength, ErrorMessage = "Не длиннее {1} символов.")]
    [Display(Name = "Город")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите адрес.")]
    [StringLength(PlaceOrderCommandValidator.MaxStreetLength, ErrorMessage = "Не длиннее {1} символов.")]
    [Display(Name = "Улица, дом, квартира")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите индекс.")]
    [StringLength(PlaceOrderCommandValidator.MaxPostalCodeLength, ErrorMessage = "Не длиннее {1} символов.")]
    [Display(Name = "Индекс")]
    public string PostalCode { get; set; } = string.Empty;

    public CardDetailsInputModel Card { get; set; } = new();

    /// <summary>Выдаётся формой: повторная отправка не заводит второй заказ и второй платёж (§7.6).</summary>
    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;

    [BindNever]
    [ValidateNever]
    public CartDto Cart { get; set; } = CartDto.Empty;
}