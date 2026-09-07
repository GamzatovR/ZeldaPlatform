using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Common.Entities;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class NotificationConfiguration : EntityConfiguration<Notification>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.Property(notification => notification.PayloadJson).HasColumnType("jsonb");
        builder.Property(notification => notification.Url).HasMaxLength(500);

        // Счётчик непрочитанных в колокольчике читается именно этим индексом.
        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.IsRead,
            notification.CreatedAt,
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}