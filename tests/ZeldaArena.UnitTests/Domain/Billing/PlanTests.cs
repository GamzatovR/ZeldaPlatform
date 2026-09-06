using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Billing;

public class PlanTests
{
    [Fact]
    public void Paid_plan_requires_duration()
    {
        Should.Throw<InvariantViolationException>(
                () => Plan.Create(PlanCodes.ProMonth, "Pro", new Money(299m, "RUB"), durationDays: 0))
            .Code.ShouldBe("plan.paid_without_duration");
    }

    [Fact]
    public void Free_plan_may_be_endless()
    {
        var plan = Plan.Create(PlanCodes.Free, "Free", Money.Zero(), durationDays: 0);

        plan.IsFree.ShouldBeTrue();
        plan.PlanFeatures.ShouldBeEmpty();
    }

    [Fact]
    public void Feature_is_granted_once_and_its_value_is_updated_on_repeat()
    {
        var plan = ProMonth();
        var featureId = Guid.CreateVersion7();

        plan.GrantFeature(featureId, "1");
        plan.GrantFeature(featureId, "3");

        plan.PlanFeatures.ShouldHaveSingleItem().Value.ShouldBe("3");
    }

    [Fact]
    public void Feature_can_be_revoked_which_is_the_core_of_EP4()
    {
        // Снять stats.advanced с тарифа Pro и продать отдельно — сценарий защиты (docs/SPEC.md §5.4).
        var plan = ProMonth();
        var teamCreate = Guid.CreateVersion7();
        var statsAdvanced = Guid.CreateVersion7();
        plan.GrantFeature(teamCreate);
        plan.GrantFeature(statsAdvanced);

        plan.RevokeFeature(statsAdvanced);

        plan.PlanFeatures.ShouldHaveSingleItem().FeatureId.ShouldBe(teamCreate);
    }

    [Fact]
    public void Revoking_a_feature_that_was_never_granted_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => ProMonth().RevokeFeature(Guid.CreateVersion7()))
            .Code.ShouldBe("plan.feature_not_granted");
    }

    [Fact]
    public void Feature_code_is_normalized()
    {
        Feature.Create("  Team.Create  ", "Своя команда").Code.ShouldBe(FeatureCodes.TeamCreate);
    }

    private static Plan ProMonth() =>
        Plan.Create(PlanCodes.ProMonth, "Pro на месяц", new Money(299m, "RUB"), durationDays: 30);
}