using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Billing.Commands.UpdateFeature;

namespace ZeldaArena.Web.Areas.Admin.Models.Billing;

public sealed class FeatureFormViewModel
{
    public const string CodePattern = "^[a-z][a-z0-9]*(\\.[a-z0-9]+)*$";

    [Required(ErrorMessage = "Укажите код функции.")]
    [StringLength(64, ErrorMessage = "Код не длиннее {1} символов.")]
    [RegularExpression(CodePattern, ErrorMessage = "Код — строчная латиница и точки, например stats.advanced.")]
    [Display(Name = "Код")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите название функции.")]
    [StringLength(UpdateFeatureCommandValidator.MaxNameLength, ErrorMessage = "Название не длиннее {1} символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [StringLength(UpdateFeatureCommandValidator.MaxDescriptionLength, ErrorMessage = "Описание не длиннее {1} символов.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }
}