using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class FeatureConfiguration : EntityConfiguration<Feature>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");

        builder.Property(feature => feature.Code).IsRequired().HasMaxLength(60);
        builder.HasIndex(feature => feature.Code).IsUnique();

        builder.Property(feature => feature.Name).IsRequired().HasMaxLength(120);
        builder.Property(feature => feature.Description).HasMaxLength(600);

        builder.HasMany(feature => feature.PlanFeatures)
            .WithOne(planFeature => planFeature.Feature)
            .HasForeignKey(planFeature => planFeature.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(feature => feature.PlanFeatures)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}