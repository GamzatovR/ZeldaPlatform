using System.ComponentModel.DataAnnotations;

using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Models.Teams;

/// <summary>
/// Форма создания команды: профиль и необязательный логотип (docs/SPEC.md §9.3, п. 8).
/// Клиентский уровень двухуровневой валидации (§15); поля профиля — общие с правкой
/// в кабинете капитана (<see cref="TeamProfileViewModel"/>).
/// </summary>
public sealed class CreateTeamViewModel : TeamProfileViewModel
{
    [ImageFile]
    [Display(Name = "Логотип")]
    public IFormFile? Logo { get; set; }
}