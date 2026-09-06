using ZeldaArena.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Единственное место во всём Web, где допустимы типы Infrastructure (docs/SPEC.md §5.2, правило 3).
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

/// <summary>
/// Точка входа объявлена явно, чтобы тестовые проекты видели сборку Web
/// (архитектурные тесты сейчас, WebApplicationFactory в дальнейшем).
/// </summary>
public partial class Program;