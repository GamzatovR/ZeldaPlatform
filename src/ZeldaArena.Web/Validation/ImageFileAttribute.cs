using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using ZeldaArena.Application.Common.Files;

namespace ZeldaArena.Web.Validation;

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