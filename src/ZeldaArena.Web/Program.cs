using ZeldaArena.Application;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Infrastructure;
using ZeldaArena.Infrastructure.Persistence.Ef;
using ZeldaArena.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Страницы аккаунта — Razor Pages в области Identity (docs/SPEC.md §5.3, §9.3 п. 16).
builder.Services.AddRazorPages();

// Каркас локализации: ресурсы лежат в Resources/, нейтральный файл заполнен по-русски.
// UseRequestLocalization, en.resx и переключатель языка — Фаза 11 (§9.5).
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Сценарии и конвейер MediatR. Типы Application в Web разрешены:
// правило 3 §5.2 закрывает только типы Infrastructure.
builder.Services.AddApplication();

// Единственное место во всём Web, где допустимы типы Infrastructure (docs/SPEC.md §5.2, правило 3).
builder.Services.AddInfrastructure(builder.Configuration);

// Письма аккаунта собираются здесь, а не в Infrastructure: текст берётся из тех же
// ресурсов, что и страницы, а ссылка — из таблицы маршрутов (docs/adr/ADR-0006).
builder.Services.AddScoped<IAccountEmailSender, AccountEmailSender>();

// Маршруты страниц входа и отказа в доступе задаются здесь, а не в AddInfrastructure:
// адреса страниц — знание слоя представления.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Миграции и сид в разработке: `docker compose up -d` плюс `dotnet run` дают
// работающую базу с нуля. В остальных средах миграции применяются отдельным шагом.
if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateAndSeedAsync();
}

// Порядок middleware зафиксирован в docs/SPEC.md §14.1:
// Exception → HSTS/HTTPS → SecurityHeaders → CorrelationId → StaticFiles + Compression →
// Routing → RateLimiter → Localization → Authentication → Authorization → CartCookie → Endpoints.
// Собственные middleware добавляются в Фазе 11 строго в этих позициях.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

/// <summary>
/// Точка входа объявлена явно, чтобы тестовые проекты видели сборку Web
/// (архитектурные тесты сейчас, WebApplicationFactory в дальнейшем).
/// </summary>
public partial class Program;