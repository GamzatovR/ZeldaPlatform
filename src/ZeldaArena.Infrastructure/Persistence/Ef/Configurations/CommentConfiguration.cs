using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class CommentConfiguration : EntityConfiguration<Comment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.Property(comment => comment.Text).IsRequired().HasMaxLength(Comment.MaxLength);

        // Мягкое удаление: удалённые комментарии не попадают ни в один запрос,
        // но остаются в базе для аудита (docs/SPEC.md §6).
        builder.HasQueryFilter(comment => !comment.IsDeleted);

        builder.HasIndex(comment => new { comment.TargetType, comment.TargetId, comment.CreatedAt });
        builder.HasIndex(comment => new { comment.IsApproved, comment.CreatedAt });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}