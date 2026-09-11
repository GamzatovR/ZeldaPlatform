using System.Text.RegularExpressions;

using FluentValidation;

using ZeldaArena.Application.Common.Files;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Common.Validation;

/// <summary>
/// Правила полей команды и игрока — общие для создания команды, правки профиля
/// и добавления игрока. Длины совпадают с ограничениями столбцов
/// (<c>TeamConfiguration</c>, <c>PlayerConfiguration</c>), тег — с инвариантом
/// <c>Team</c>: форма не должна принимать то, что потом отвергнет база или домен.
/// </summary>
public static partial class EsportsValidationRules
{
    public const int MaxTeamNameLength = 120;
    public const int MinTagLength = 2;
    public const int MaxTagLength = 8;
    public const int MaxDescriptionLength = 4000;
    public const int MaxNicknameLength = 60;
    public const int MaxPersonNameLength = 80;

    /// <summary>Тег — латиница и цифры: он стоит в карточке матча и в таблицах рядом с названием.</summary>
    public const string TagPattern = "^[A-Za-z0-9]{2,8}$";

    public static IRuleBuilderOptions<T, string> ValidTeamName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите название команды.")
            .MaximumLength(MaxTeamNameLength)
            .WithMessage($"Название не длиннее {MaxTeamNameLength} символов.");

    public static IRuleBuilderOptions<T, string> ValidTag<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите тег команды.")
            .Matches(TagRegex())
            .WithMessage($"Тег — от {MinTagLength} до {MaxTagLength} латинских букв или цифр.");

    public static IRuleBuilderOptions<T, string> ValidCountryCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите страну.")
            .Must(code => CountryCode.TryFrom(code, out _))
            .WithMessage("Код страны — две латинские буквы ISO 3166, например RU.");

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(MaxDescriptionLength)
            .WithMessage($"Описание не длиннее {MaxDescriptionLength} символов.");

    public static IRuleBuilderOptions<T, string> ValidNickname<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .WithMessage("Укажите ник игрока.")
            .MaximumLength(MaxNicknameLength)
            .WithMessage($"Ник не длиннее {MaxNicknameLength} символов.");

    /// <summary>
    /// Заявленные расширение и размер. Содержимое по сигнатуре проверяет сценарий
    /// и перепроверяет хранилище: валидатор потока не читает.
    /// </summary>
    public static IRuleBuilderOptions<T, FileUpload?> ValidImageUpload<T>(this IRuleBuilder<T, FileUpload?> rule) =>
        rule.Must(file => file is null || (file.Length > 0 && file.Length <= ImageUploadRules.MaxSizeBytes))
            .WithMessage($"Изображение не больше {ImageUploadRules.MaxSizeBytes / 1024 / 1024} МБ.")
            .Must(file => file is null || ImageUploadRules.HasAllowedExtension(file.FileName))
            .WithMessage("Подходят только изображения PNG, JPEG и WebP.");

    [GeneratedRegex(TagPattern)]
    private static partial Regex TagRegex();
}