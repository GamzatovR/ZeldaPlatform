using System.Text.Encodings.Web;
using System.Text.Unicode;

using Microsoft.Extensions.WebEncoders;

using ZeldaArena.Application;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Infrastructure;
using ZeldaArena.Infrastructure.Persistence.Ef;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Middleware;
using ZeldaArena.Web.RateLimiting;
using ZeldaArena.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Каркас локализации: ресурсы лежат в Resources/, нейтральный файл заполнен по-русски.
// AddViewLocalization даёт вьюхам IHtmlLocalizer, AddDataAnnotationsLocalization —
// переводимые сообщения валидации. UseRequestLocalization, en.resx и переключатель
// языка — Фаза 11 (docs/SPEC.md §9.5).
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Razor по умолчанию экранирует всё, что вне латиницы: «Подписка» превращается
// в «&#x41F;&#x43E;...» и страница на русском раздувается в несколько раз. Экранирование
// защищает от подстановки разметки, а не от кириллицы, поэтому разрешаем весь Unicode —
// иначе требование §16 по скорости загрузки не выполнить.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Страницы аккаунта — Razor Pages в области Identity (docs/SPEC.md §5.3, §9.3 п. 16).
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Сценарии и конвейер MediatR. Типы Application в Web разрешены:
// правило 3 §5.2 закрывает только типы Infrastructure.
builder.Services.AddApplication();

// Единственное место во всём Web, где допустимы типы Infrastructure (docs/SPEC.md §5.2, правило 3).
builder.Services.AddInfrastructure(builder.Configuration);

// Письма аккаунта собираются здесь, а не в Infrastructure: текст берётся из тех же
// ресурсов, что и страницы, а ссылка — из таблицы маршрутов (docs/adr/ADR-0006).
builder.Services.AddScoped<IAccountEmailSender, AccountEmailSender>();
builder.Services.AddScoped<IBillingEmailSender, BillingEmailSender>();

// Гостевая корзина опознаётся по подписанной куке — это знание слоя представления,
// сценарии получают уже проверенный идентификатор (docs/SPEC.md §14.1).
builder.Services.AddScoped<IGuestCartIdentity, CookieGuestCartIdentity>();

// Маршруты страниц входа и отказа в доступе задаются здесь, а не в AddInfrastructure:
// адреса страниц — знание слоя представления.
// Статические политики §8.1 плюс динамические Feature:{code} (§7.3).
builder.Services.AddPlatformAuthorization();

// Ограничение частоты на формах аккаунта (§8.2).
builder.Services.AddPlatformRateLimiter();

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
// Собственные middleware добавляются строго в этих позициях: CartCookie — с Фазы 7,
// остальные — в Фазе 11.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// После аутентификации: слиянию гостевой корзины нужен уже известный пользователь.
// После маршрутизации: кука выдаётся только страницам, а не статическим файлам.
app.UseMiddleware<CartCookieMiddleware>();

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