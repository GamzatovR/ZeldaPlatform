using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Validation;

/// <summary>
/// Клиентский уровень проверки загружаемого изображения (docs/SPEC.md §15): расширение
/// из whitelist и размер. Правила берутся из <see cref="ImageUploadRules"/> — тех же,
/// что читает валидатор сценария и хранилище, поэтому форма не примет файл,
/// который отвергнет сервер, и наоборот.
///
/// Содержимое по сигнатуре здесь не проверяется и проверяться не может: браузер
/// отправляет файл целиком, а читать его в JavaScript ради magic bytes бессмысленно —
/// сервер сделает это всё равно (§15, «сервер валидирует всегда»).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class ImageFileAttribute : ValidationAttribute, IClientModelValidator
{
    public ImageFileAttribute()
        : base("Подходят только изображения PNG, JPEG и WebP не больше 2 МБ.")
    {
    }

    public override bool IsValid(object? value) =>
        value is not IFormFile file
        || (file.Length > 0
            && file.Length <= ImageUploadRules.MaxSizeBytes
            && ImageUploadRules.HasAllowedExtension(file.FileName));

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-imagefile", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        context.Attributes.TryAdd("data-val-imagefile-extensions", ImageUploadRules.AcceptAttribute);
        context.Attributes.TryAdd(
            "data-val-imagefile-maxsize",
            ImageUploadRules.MaxSizeBytes.ToString(CultureInfo.InvariantCulture));
    }
}