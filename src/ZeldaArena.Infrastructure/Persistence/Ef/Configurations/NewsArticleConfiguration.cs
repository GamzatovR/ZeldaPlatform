using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ZeldaArena.Domain.Esports;
using ZeldaArena.Infrastructure.Identity;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Configurations;

public sealed class NewsArticleConfiguration : EntityConfiguration<NewsArticle>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("NewsArticles");

        builder.Property(article => article.Slug).IsRequired();
        builder.HasIndex(article => article.Slug).IsUnique();

        builder.Property(article => article.Title).IsRequired().HasMaxLength(250);
        builder.Property(article => article.Summary).HasMaxLength(600);
        builder.Property(article => article.BodyHtml).IsRequired();
        builder.Property(article => article.CoverPath).HasMaxLength(400);

        builder.HasIndex(article => new { article.IsPublished, article.PublishedAt });

        // Restrict: удаление автора не должно уносить новости портала.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(article => article.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}