using Microsoft.EntityFrameworkCore;
using parcial.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging básico
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configurar base de datos OPCIONAL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                      ?? "DataSource=app.db;Cache=Shared";

Console.WriteLine($"Using connection string: {connectionString}");

// Comentar DB por ahora para aislar el problema
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
// {
//     options.UseSqlite(connectionString);
// });

// MVC básico SOLAMENTE
builder.Services.AddControllersWithViews();

var app = builder.Build();

// NO inicializar base de datos por ahora
Console.WriteLine("Skipping database setup for now...");

// Pipeline ultra simple - SIN manejo de errores complejo
app.UseStaticFiles();
app.UseRouting();

// Mapear rutas básicas
app.MapGet("/", () => "Portal Universitario - Aplicación funcionando!");
app.MapGet("/health", () => "OK");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("App starting...");
app.Run();
