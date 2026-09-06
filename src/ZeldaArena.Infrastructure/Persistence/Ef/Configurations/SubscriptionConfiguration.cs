using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Billing;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class SubscriptionConfiguration : EntityConfiguration<Subscription>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.UseXminAsConcurrencyToken();

        builder.HasOne(subscription => subscription.Plan)
            .WithMany()
            .HasForeignKey(subscription => subscription.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(subscription => subscription.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы из docs/SPEC.md §6: проверка прав идёт по (UserId, Status),
        // фоновая служба ищет истёкшие по EndsAt.
        builder.HasIndex(subscription => new { subscription.UserId, subscription.Status });
        builder.HasIndex(subscription => subscription.EndsAt);
    }
}