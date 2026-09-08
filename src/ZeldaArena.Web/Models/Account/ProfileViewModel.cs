using System.ComponentModel.DataAnnotations;

namespace ZeldaArena.Web.Models.Account;

public sealed class ProfileViewModel
{
    [StringLength(64, ErrorMessage = "Отображаемое имя не длиннее {1} символов.")]
    [Display(Name = "Отображаемое имя")]
    public string? DisplayName { get; set; }

    // Длина берётся у доменного объекта-значения, но по полному имени: короткое
    // CountryCode здесь уже занято самим свойством.
    [StringLength(
        Domain.ValueObjects.CountryCode.Length,
        MinimumLength = Domain.ValueObjects.CountryCode.Length,
        ErrorMessage = "Код страны состоит из двух латинских букв, например RU.")]
    [Display(Name = "Страна")]
    public string? CountryCode { get; set; }

    [Display(Name = "Язык интерфейса")]
    public string? PreferredCulture { get; set; }
}