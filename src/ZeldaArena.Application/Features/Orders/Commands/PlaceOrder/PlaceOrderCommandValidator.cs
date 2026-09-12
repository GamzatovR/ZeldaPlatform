using FluentValidation;

using ZeldaArena.Application.Features.Payments;

namespace ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public const int MaxRecipientLength = 150;
    public const int MinPhoneLength = 5;
    public const int MaxPhoneLength = 30;
    public const int MaxCountryLength = 60;
    public const int MaxCityLength = 120;
    public const int MaxStreetLength = 250;
    public const int MaxPostalCodeLength = 20;

    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.Recipient)
            .NotEmpty().WithMessage("Укажите получателя.")
            .MaximumLength(MaxRecipientLength).WithMessage($"Имя получателя не длиннее {MaxRecipientLength} символов.");

        RuleFor(command => command.Phone)
            .NotEmpty().WithMessage("Укажите телефон.")
            .Must(phone => phone is null || phone.Trim().Length >= MinPhoneLength)
            .WithMessage($"Телефон не короче {MinPhoneLength} символов.")
            .MaximumLength(MaxPhoneLength).WithMessage($"Телефон не длиннее {MaxPhoneLength} символов.");

        RuleFor(command => command.Country)
            .NotEmpty().WithMessage("Укажите страну.")
            .MaximumLength(MaxCountryLength).WithMessage($"Страна не длиннее {MaxCountryLength} символов.");

        RuleFor(command => command.City)
            .NotEmpty().WithMessage("Укажите город.")
            .MaximumLength(MaxCityLength).WithMessage($"Город не длиннее {MaxCityLength} символов.");

        RuleFor(command => command.Street)
            .NotEmpty().WithMessage("Укажите адрес.")
            .MaximumLength(MaxStreetLength).WithMessage($"Адрес не длиннее {MaxStreetLength} символов.");

        RuleFor(command => command.PostalCode)
            .NotEmpty().WithMessage("Укажите индекс.")
            .MaximumLength(MaxPostalCodeLength).WithMessage($"Индекс не длиннее {MaxPostalCodeLength} символов.");

        this.AddCardPaymentRules();
    }
}