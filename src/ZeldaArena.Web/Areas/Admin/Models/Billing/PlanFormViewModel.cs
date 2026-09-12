using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Billing;
using ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

namespace ZeldaArena.Web.Areas.Admin.Models.Billing;

public sealed class PlanFormViewModel
{
    [Required(ErrorMessage = "Укажите код тарифа.")]
    [StringLength(PlanFieldsValidator.MaxCodeLength, ErrorMessage = "Код не длиннее {1} символов.")]
    [RegularExpression(PlanFieldsValidator.CodePattern, ErrorMessage = "Код — латиница, цифры и дефисы, например pro-month.")]
    [Display(Name = "Код")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите название тарифа.")]
    [StringLength(PlanFieldsValidator.MaxNameLength, ErrorMessage = "Название не длиннее {1} символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [StringLength(PlanFieldsValidator.MaxDescriptionLength, ErrorMessage = "Описание не длиннее {1} символов.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Цена — от {1} до {2}.")]
    [Display(Name = "Цена, ₽")]
    public decimal Price { get; set; }

    [Range(0, PlanFieldsValidator.MaxDurationDays, ErrorMessage = "Срок — от {1} до {2} дней.")]
    [Display(Name = "Срок, дней")]
    public int DurationDays { get; set; } = 30;

    [Range(0, 1000, ErrorMessage = "Порядок — от {1} до {2}.")]
    [Display(Name = "Порядок в списке")]
    public int SortOrder { get; set; }

    [Display(Name = "В продаже")]
    public bool IsActive { get; set; } = true;

    public static PlanFormViewModel From(PlanEditDto plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new PlanFormViewModel
        {
            Code = plan.Code,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            DurationDays = plan.DurationDays,
            SortOrder = plan.SortOrder,
            IsActive = plan.IsActive,
        };
    }
}