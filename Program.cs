using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using parcial.Data;
using parcial.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Configurar base de datos SQLite con persistencia en Render
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Para Render, usar directorio persistente si está disponible
if (string.IsNullOrEmpty(connectionString))
{
    var dataDir = "/opt/render/project/data";
    try 
    {
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        connectionString = $"DataSource={dataDir}/app.db;Cache=Shared";
        Console.WriteLine($"📁 Usando directorio de datos: {dataDir}");
    }
    catch
    {
        // Fallback a directorio temporal
        connectionString = "DataSource=/tmp/app.db;Cache=Shared";
        Console.WriteLine("📁 Usando directorio temporal para base de datos");
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    
    // Configuraciones para producción
    if (builder.Environment.IsProduction())
    {
        options.EnableSensitiveDataLogging(false);
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    // Relajar requisitos de contraseña
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Configurar autorización
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.LoginPath = "/Identity/Account/Login";
});

// Configurar cache distribuido con fallback
try 
{
    // Intentar Redis en producción si está disponible
    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") 
                   ?? Environment.GetEnvironmentVariable("ConnectionStrings__Redis")
                   ?? builder.Configuration.GetConnectionString("Redis");
    
    if (!string.IsNullOrEmpty(redisUrl) && builder.Environment.IsProduction())
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisUrl;
            options.InstanceName = "ParcialUniversidad";
        });
        Console.WriteLine("✅ Redis configurado para producción");
    }
    else
    {
        // Fallback a memoria cache
        builder.Services.AddDistributedMemoryCache();
        Console.WriteLine("📝 Usando cache en memoria (fallback)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error configurando Redis: {ex.Message}");
    Console.WriteLine("📝 Usando cache en memoria como fallback");
    builder.Services.AddDistributedMemoryCache();
}

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ParcialSession";
});

// Registrar servicios personalizados
builder.Services.AddScoped<IMatriculaService, MatriculaService>();
builder.Services.AddScoped<ISesionService, SesionService>();
builder.Services.AddScoped<ICursosCache, CursosCache>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configurar puerto dinámico para Render
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Deshabilitar HTTPS redirect en contenedores
// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configurar sesiones
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Endpoints de salud para Render
app.MapGet("/health", () => "OK");
app.MapGet("/health/ready", async (ApplicationDbContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database not ready: {ex.Message}");
    }
});

app.MapGet("/health/live", () => Results.Ok(new { status = "Alive", timestamp = DateTime.UtcNow }));

Console.WriteLine("🚀 Portal Universitario iniciando...");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Listening on: {string.Join(", ", app.Urls)}");

// Ejecutar seed data y migraciones (en background para no bloquear el inicio)
_ = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Esperar un poco para que la aplicación esté lista
        await Task.Delay(3000);
        
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Aplicar migraciones pendientes
        logger.LogInformation("🔄 Aplicando migraciones de base de datos...");
        await context.Database.MigrateAsync();
        
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("🌱 Ejecutando seed data...");
        await ApplicationDbContext.SeedDataAsync(context, userManager, roleManager);
        
        logger.LogInformation("✅ Inicialización de base de datos completada exitosamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error durante la inicialización de la base de datos: {Error}", ex.Message);
    }
});

app.Run();
