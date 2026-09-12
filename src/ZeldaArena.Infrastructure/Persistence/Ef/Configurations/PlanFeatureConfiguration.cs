using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

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