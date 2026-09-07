using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

/// <summary>
/// Привязка фич к тарифам: составной ключ, редактируется чекбоксами в админке.
/// Именно здесь живёт расширяемость подписок (docs/SPEC.md §5.4, EP-4 и EP-5).
/// </summary>
public sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("PlanFeatures");

        builder.HasKey(planFeature => new { planFeature.PlanId, planFeature.FeatureId });

        builder.Property(planFeature => planFeature.Value).HasMaxLength(200);

        builder.HasIndex(planFeature => planFeature.FeatureId);
    }
}