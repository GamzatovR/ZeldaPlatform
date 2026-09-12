using System.ComponentModel.DataAnnotations;

using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Models.Teams;

public sealed class CreateTeamViewModel : TeamProfileViewModel
{
    [ImageFile]
    [Display(Name = "Логотип")]
    public IFormFile? Logo { get; set; }
}