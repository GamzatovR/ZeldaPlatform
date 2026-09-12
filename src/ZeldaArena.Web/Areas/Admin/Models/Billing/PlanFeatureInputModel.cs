namespace ZeldaArena.Web.Areas.Admin.Models.Billing;

public sealed class PlanFeatureInputModel
{
    public Guid FeatureId { get; set; }

    public bool IsGranted { get; set; }

    public string? Value { get; set; }
}