using System.Text.Encodings.Web;
using System.Text.Unicode;

using Microsoft.Extensions.WebEncoders;

using ZeldaArena.Application;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Infrastructure;
using ZeldaArena.Infrastructure.Persistence.Ef;
using ZeldaArena.Web.Areas.Admin;
using ZeldaArena.Web.Areas.Api;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Middleware;
using ZeldaArena.Web.RateLimiting;
using ZeldaArena.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Каркас локализации.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Razor по умолчанию экранирует всё, что вне латиницы.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Доступ ко всей админке — одно соглашение, а не атрибут на каждом контроллере.
builder.Services.AddControllersWithViews(options => options.Conventions.Add(new AdminAreaConvention()))
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Страницы аккаунта — Razor Pages в области Identity.
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Сценарии и конвейер MediatR. Типы Application в Web разрешены:
// правило 3 закрывает только типы Infrastructure.
builder.Services.AddApplication();

// Единственное место во всём Web, где допустимы типы Infrastructure.
builder.Services.AddInfrastructure(builder.Configuration);

// Письма аккаунта собираются здесь, а не в Infrastructure: текст берётся из тех же
// ресурсов, что и страницы, а ссылка — из таблицы маршрутов (docs/adr/ADR-0006).
builder.Services.AddScoped<IAccountEmailSender, AccountEmailSender>();
builder.Services.AddScoped<IBillingEmailSender, BillingEmailSender>();

// Гостевая корзина опознаётся по подписанной куке — это знание слоя представления,
// сценарии получают уже проверенный идентификатор.
builder.Services.AddScoped<IGuestCartIdentity, CookieGuestCartIdentity>();

// Маршруты страниц входа и отказа в доступе задаются здесь, а не в AddInfrastructure.
builder.Services.AddPlatformAuthorization();

// Ограничение частоты на формах аккаунта.
builder.Services.AddPlatformRateLimiter();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    // fetch к Areas/Api получает 401/403, а не HTML страницы входа.
    options.AnswerApiWithStatusCodes();
});

var app = builder.Build();

// Миграции и сид в разработке: `docker compose up -d` плюс `dotnet run` дают
// работающую базу с нуля. В остальных средах миграции применяются отдельным шагом.
if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateAndSeedAsync();
}

// Порядок middleware: Exception → HSTS → SecurityHeaders → CorrelationId → StaticFiles →
// Compression → Routing → RateLimiter → Localization → Auth → CartCookie → Endpoints.
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

public partial class Program;