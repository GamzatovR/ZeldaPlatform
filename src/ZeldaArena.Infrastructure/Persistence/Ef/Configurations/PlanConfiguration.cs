using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class PlanConfiguration : EntityConfiguration<Plan>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.Property(plan => plan.Code).IsRequired().HasMaxLength(60);
        builder.HasIndex(plan => plan.Code).IsUnique();

        builder.Property(plan => plan.Name).IsRequired().HasMaxLength(120);
        builder.Property(plan => plan.Description).HasMaxLength(600);

        builder.Ignore(plan => plan.IsFree);

        builder.ComplexProperty(plan => plan.Price, money =>
        {
            money.Property(amount => amount.Amount).HasColumnName("Price");
            money.Property(currency => currency.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsFixedLength();
        });

        builder.UseXminAsConcurrencyToken();

        builder.HasMany(plan => plan.PlanFeatures)
            .WithOne(planFeature => planFeature.Plan)
            .HasForeignKey(planFeature => planFeature.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Состав фич — часть агрегата, а не связанные данные.
        builder.Navigation(plan => plan.PlanFeatures)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(plan => new { plan.IsActive, plan.SortOrder });
    }
}