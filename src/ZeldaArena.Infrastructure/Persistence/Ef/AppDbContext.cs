using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common.Entities;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.Infrastructure.Identity;
using ZeldaArena.Infrastructure.Persistence.Ef.Converters;

namespace ZeldaArena.Infrastructure.Persistence.Ef;

/// <summary>
/// Контекст основной базы (PostgreSQL). Наследует IdentityDbContext, поэтому первая
/// миграция содержит и семь таблиц AspNet*, и предметную модель — внешние ключи
/// на пользователя настоящие с самого начала (docs/adr/ADR-0003).
///
/// Конфигурации задаются только через IEntityTypeConfiguration и подхватываются
/// по сборке: правил маппинга внутри самого контекста нет (docs/SPEC.md §6).
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tournament> Tournaments => Set<Tournament>();

    public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<RosterEntry> RosterEntries => Set<RosterEntry>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<PlayerMatchStats> PlayerMatchStats => Set<PlayerMatchStats>();

    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Follow> Follows => Set<Follow>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<Feature> Features => Set<Feature>();

    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<ContentTranslation> ContentTranslations => Set<ContentTranslation>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Сквозные соглашения: деньги — numeric(18,2) (docs/SPEC.md §6), объекты-значения
    /// с одним полем хранятся строкой. Повторять это в каждой конфигурации не нужно.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 2);

        configurationBuilder.Properties<Slug>()
            .HaveConversion<SlugConverter, SlugComparer>()
            .HaveMaxLength(Slug.MaxLength);

        configurationBuilder.Properties<CountryCode>()
            .HaveConversion<CountryCodeConverter, CountryCodeComparer>()
            .HaveMaxLength(CountryCode.Length)
            .AreFixedLength();
    }
}