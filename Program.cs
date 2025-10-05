using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// Configurar autenticación por cookies como esquema por defecto
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "ParcialAuth";
    });

builder.Services.AddControllersWithViews();

// Configurar autorización
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar cache distribuido con fallback robusto
bool useRedis = false;

// Verificar configuración explícita para usar Redis
var useRedisConfig = builder.Configuration.GetValue<bool>("CacheSettings:UseRedis");
var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") 
               ?? Environment.GetEnvironmentVariable("ConnectionStrings__Redis")
               ?? builder.Configuration.GetConnectionString("Redis");

// Solo usar Redis si está explícitamente habilitado Y configurado
if (useRedisConfig && !string.IsNullOrEmpty(redisUrl))
{
    try 
    {
        // Verificar que no sea localhost (que causaría error en Render)
        if (!redisUrl.Contains("localhost") && !redisUrl.Contains("127.0.0.1"))
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisUrl;
                options.InstanceName = "ParcialUniversidad";
            });
            useRedis = true;
            Console.WriteLine("✅ Redis configurado para producción");
        }
        else
        {
            Console.WriteLine("⚠️ Redis URL contiene localhost, usando cache en memoria");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error configurando Redis: {ex.Message}");
        Console.WriteLine("📝 Usando cache en memoria como fallback");
    }
}
else
{
    Console.WriteLine("📝 Redis deshabilitado por configuración");
}

if (!useRedis)
{
    // Usar cache en memoria como fallback seguro
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("📝 Usando cache en memoria");
}

// Configurar sesiones (sin depender de cache distribuido)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "ParcialSession";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Configurar Data Protection para evitar warnings en contenedores
if (builder.Environment.IsProduction())
{
    var keysDir = "/opt/render/project/data/keys";
    try
    {
        if (!Directory.Exists(keysDir))
        {
            Directory.CreateDirectory(keysDir);
            Console.WriteLine($"📁 Directorio de claves creado: {keysDir}");
        }
        
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        
        Console.WriteLine("🔐 Data Protection configurado con persistencia");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error configurando Data Protection: {ex.Message}");
        Console.WriteLine("🔐 Usando Data Protection por defecto");
    }
}
else
{
    Console.WriteLine("🔐 Data Protection en modo desarrollo");
}

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
