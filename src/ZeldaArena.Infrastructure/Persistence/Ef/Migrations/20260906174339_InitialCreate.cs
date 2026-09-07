using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ZeldaArena.Infrastructure.Persistence.Ef.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppSettings",
            columns: table => new
            {
                Key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Value = table.Column<string>(type: "text", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppSettings", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DisplayName = table.Column<string>(type: "text", nullable: true),
                AvatarPath = table.Column<string>(type: "text", nullable: true),
                PreferredCulture = table.Column<string>(type: "text", nullable: true),
                CountryCode = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContentTranslations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EntityType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                CultureCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                FieldName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                Value = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentTranslations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Features",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Features", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Plans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                DurationDays = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Plans", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Players",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Nickname = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                FirstName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                LastName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                Country = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                Role = table.Column<int>(type: "integer", nullable: false),
                AvatarPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                Bio = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Players", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ProductCategories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCategories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tournaments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                Tier = table.Column<int>(type: "integer", nullable: false),
                Region = table.Column<int>(type: "integer", nullable: false),
                StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                RulesHtml = table.Column<string>(type: "text", nullable: true),
                LogoPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                BannerPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                PrizePool = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tournaments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                ProviderKey = table.Column<string>(type: "text", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                LoginProvider = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                AnonymousId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Carts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Carts_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetType = table.Column<int>(type: "integer", nullable: false),
                TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comments_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Follows",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetType = table.Column<int>(type: "integer", nullable: false),
                TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Follows", x => x.Id);
                table.ForeignKey(
                    name: "FK_Follows_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NewsArticles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                Summary = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                BodyHtml = table.Column<string>(type: "text", nullable: false),
                CoverPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                ViewCount = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NewsArticles", x => x.Id);
                table.ForeignKey(
                    name: "FK_NewsArticles_AspNetUsers_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                IsRead = table.Column<bool>(type: "boolean", nullable: false),
                ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_Notifications_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Country = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Recipient = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Street = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Teams",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Tag = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                LogoPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                Country = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                Region = table.Column<int>(type: "integer", nullable: false),
                FoundedAt = table.Column<DateOnly>(type: "date", nullable: true),
                Rating = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Teams", x => x.Id);
                table.ForeignKey(
                    name: "FK_Teams_AspNetUsers_OwnerUserId",
                    column: x => x.OwnerUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "PlanFeatures",
            columns: table => new
            {
                PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                FeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlanFeatures", x => new { x.PlanId, x.FeatureId });
                table.ForeignKey(
                    name: "FK_PlanFeatures_Features_FeatureId",
                    column: x => x.FeatureId,
                    principalTable: "Features",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlanFeatures_Plans_PlanId",
                    column: x => x.PlanId,
                    principalTable: "Plans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                PriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subscriptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Subscriptions_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Subscriptions_Plans_PlanId",
                    column: x => x.PlanId,
                    principalTable: "Plans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                StockQuantity = table.Column<int>(type: "integer", nullable: false),
                ImagePath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_ProductCategories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "ProductCategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Matches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                TeamAId = table.Column<Guid>(type: "uuid", nullable: false),
                TeamBId = table.Column<Guid>(type: "uuid", nullable: false),
                ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                BestOf = table.Column<int>(type: "integer", nullable: false),
                ScoreA = table.Column<int>(type: "integer", nullable: false),
                ScoreB = table.Column<int>(type: "integer", nullable: false),
                WinnerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                StreamUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Matches", x => x.Id);
                table.ForeignKey(
                    name: "FK_Matches_Teams_TeamAId",
                    column: x => x.TeamAId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Matches_Teams_TeamBId",
                    column: x => x.TeamBId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Matches_Tournaments_TournamentId",
                    column: x => x.TournamentId,
                    principalTable: "Tournaments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RosterEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                LeftAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RosterEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_RosterEntries_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RosterEntries_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TournamentTeams",
            columns: table => new
            {
                TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                Seed = table.Column<int>(type: "integer", nullable: false),
                Placement = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TournamentTeams", x => new { x.TournamentId, x.TeamId });
                table.ForeignKey(
                    name: "FK_TournamentTeams_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TournamentTeams_Tournaments_TournamentId",
                    column: x => x.TournamentId,
                    principalTable: "Tournaments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Purpose = table.Column<int>(type: "integer", nullable: false),
                SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CardLast4 = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                CardBrand = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                ConfirmationEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                ConfirmationCodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ConfirmationExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ConfirmationAttemptsLeft = table.Column<int>(type: "integer", nullable: false),
                IdempotencyKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FailureReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Payments_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Payments_Subscriptions_SubscriptionId",
                    column: x => x.SubscriptionId,
                    principalTable: "Subscriptions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "CartItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CartId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                PriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CartItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_CartItems_Carts_CartId",
                    column: x => x.CartId,
                    principalTable: "Carts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CartItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderItems_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlayerMatchStats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                Kills = table.Column<int>(type: "integer", nullable: false),
                Deaths = table.Column<int>(type: "integer", nullable: false),
                Assists = table.Column<int>(type: "integer", nullable: false),
                Damage = table.Column<int>(type: "integer", nullable: false),
                Rating = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayerMatchStats", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlayerMatchStats_Matches_MatchId",
                    column: x => x.MatchId,
                    principalTable: "Matches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayerMatchStats_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlayerMatchStats_Teams_TeamId",
                    column: x => x.TeamId,
                    principalTable: "Teams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_CartId_ProductId",
            table: "CartItems",
            columns: new[] { "CartId", "ProductId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_ProductId",
            table: "CartItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Carts_AnonymousId",
            table: "Carts",
            column: "AnonymousId",
            unique: true,
            filter: "\"AnonymousId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Carts_UserId",
            table: "Carts",
            column: "UserId",
            unique: true,
            filter: "\"UserId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_IsApproved_CreatedAt",
            table: "Comments",
            columns: new[] { "IsApproved", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Comments_TargetType_TargetId_CreatedAt",
            table: "Comments",
            columns: new[] { "TargetType", "TargetId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Comments_UserId",
            table: "Comments",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ContentTranslations_EntityType_EntityId_CultureCode_FieldNa~",
            table: "ContentTranslations",
            columns: new[] { "EntityType", "EntityId", "CultureCode", "FieldName" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Features_Code",
            table: "Features",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Follows_TargetType_TargetId",
            table: "Follows",
            columns: new[] { "TargetType", "TargetId" });

        migrationBuilder.CreateIndex(
            name: "IX_Follows_UserId_TargetType_TargetId",
            table: "Follows",
            columns: new[] { "UserId", "TargetType", "TargetId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Matches_Status_ScheduledAt",
            table: "Matches",
            columns: new[] { "Status", "ScheduledAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Matches_TeamAId",
            table: "Matches",
            column: "TeamAId");

        migrationBuilder.CreateIndex(
            name: "IX_Matches_TeamBId",
            table: "Matches",
            column: "TeamBId");

        migrationBuilder.CreateIndex(
            name: "IX_Matches_TournamentId_Status_ScheduledAt",
            table: "Matches",
            columns: new[] { "TournamentId", "Status", "ScheduledAt" });

        migrationBuilder.CreateIndex(
            name: "IX_NewsArticles_AuthorId",
            table: "NewsArticles",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_NewsArticles_IsPublished_PublishedAt",
            table: "NewsArticles",
            columns: new[] { "IsPublished", "PublishedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_NewsArticles_Slug",
            table: "NewsArticles",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId_IsRead_CreatedAt",
            table: "Notifications",
            columns: new[] { "UserId", "IsRead", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_OrderId",
            table: "OrderItems",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItems_ProductId",
            table: "OrderItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_Number",
            table: "Orders",
            column: "Number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_Status_PlacedAt",
            table: "Orders",
            columns: new[] { "Status", "PlacedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Orders_UserId_Status_PlacedAt",
            table: "Orders",
            columns: new[] { "UserId", "Status", "PlacedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Payments_IdempotencyKey",
            table: "Payments",
            column: "IdempotencyKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Payments_OrderId",
            table: "Payments",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_SubscriptionId",
            table: "Payments",
            column: "SubscriptionId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_UserId_Status",
            table: "Payments",
            columns: new[] { "UserId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanFeatures_FeatureId",
            table: "PlanFeatures",
            column: "FeatureId");

        migrationBuilder.CreateIndex(
            name: "IX_Plans_Code",
            table: "Plans",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Plans_IsActive_SortOrder",
            table: "Plans",
            columns: new[] { "IsActive", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_PlayerMatchStats_MatchId_PlayerId",
            table: "PlayerMatchStats",
            columns: new[] { "MatchId", "PlayerId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlayerMatchStats_PlayerId",
            table: "PlayerMatchStats",
            column: "PlayerId");

        migrationBuilder.CreateIndex(
            name: "IX_PlayerMatchStats_TeamId",
            table: "PlayerMatchStats",
            column: "TeamId");

        migrationBuilder.CreateIndex(
            name: "IX_Players_Country",
            table: "Players",
            column: "Country");

        migrationBuilder.CreateIndex(
            name: "IX_Players_Nickname",
            table: "Players",
            column: "Nickname");

        migrationBuilder.CreateIndex(
            name: "IX_Players_Role",
            table: "Players",
            column: "Role");

        migrationBuilder.CreateIndex(
            name: "IX_Players_Slug",
            table: "Players",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_Slug",
            table: "ProductCategories",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryId_IsActive",
            table: "Products",
            columns: new[] { "CategoryId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Products_IsActive",
            table: "Products",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Sku",
            table: "Products",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_Slug",
            table: "Products",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RosterEntries_PlayerId_LeftAt",
            table: "RosterEntries",
            columns: new[] { "PlayerId", "LeftAt" });

        migrationBuilder.CreateIndex(
            name: "IX_RosterEntries_TeamId_LeftAt",
            table: "RosterEntries",
            columns: new[] { "TeamId", "LeftAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_EndsAt",
            table: "Subscriptions",
            column: "EndsAt");

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_PlanId",
            table: "Subscriptions",
            column: "PlanId");

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_UserId_Status",
            table: "Subscriptions",
            columns: new[] { "UserId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsApproved",
            table: "Teams",
            column: "IsApproved");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_OwnerUserId",
            table: "Teams",
            column: "OwnerUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Region_Rating",
            table: "Teams",
            columns: new[] { "Region", "Rating" });

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Slug",
            table: "Teams",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tournaments_IsFeatured",
            table: "Tournaments",
            column: "IsFeatured");

        migrationBuilder.CreateIndex(
            name: "IX_Tournaments_Region_Status",
            table: "Tournaments",
            columns: new[] { "Region", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Tournaments_Slug",
            table: "Tournaments",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tournaments_Status_StartsAt",
            table: "Tournaments",
            columns: new[] { "Status", "StartsAt" });

        migrationBuilder.CreateIndex(
            name: "IX_TournamentTeams_TeamId",
            table: "TournamentTeams",
            column: "TeamId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppSettings");

        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "CartItems");

        migrationBuilder.DropTable(
            name: "Comments");

        migrationBuilder.DropTable(
            name: "ContentTranslations");

        migrationBuilder.DropTable(
            name: "Follows");

        migrationBuilder.DropTable(
            name: "NewsArticles");

        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "OrderItems");

        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.DropTable(
            name: "PlanFeatures");

        migrationBuilder.DropTable(
            name: "PlayerMatchStats");

        migrationBuilder.DropTable(
            name: "RosterEntries");

        migrationBuilder.DropTable(
            name: "TournamentTeams");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "Carts");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "Subscriptions");

        migrationBuilder.DropTable(
            name: "Features");

        migrationBuilder.DropTable(
            name: "Matches");

        migrationBuilder.DropTable(
            name: "Players");

        migrationBuilder.DropTable(
            name: "ProductCategories");

        migrationBuilder.DropTable(
            name: "Plans");

        migrationBuilder.DropTable(
            name: "Teams");

        migrationBuilder.DropTable(
            name: "Tournaments");

        migrationBuilder.DropTable(
            name: "AspNetUsers");
    }
}