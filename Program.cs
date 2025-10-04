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

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db;Cache=Shared";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    
    // Configuraciones adicionales para producción
    if (builder.Environment.IsProduction())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableServiceProviderCaching();
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

// Configurar cache distribuido - Redis en producción, memoria en desarrollo
if (builder.Environment.IsProduction())
{
    // Redis en producción
    var redisUrl = builder.Configuration.GetConnectionString("Redis") 
                   ?? Environment.GetEnvironmentVariable("REDIS_URL") 
                   ?? "localhost:6379";
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisUrl;
        options.InstanceName = "ParcialUniversidad";
    });
}
else
{
    // Memoria en desarrollo
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

// Configurar puerto para Render
if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
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

// Ejecutar seed data y migraciones
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Aplicar migraciones pendientes
        logger.LogInformation("Aplicando migraciones de base de datos...");
        await context.Database.MigrateAsync();
        
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("Ejecutando seed data...");
        await ApplicationDbContext.SeedDataAsync(context, userManager, roleManager);
        
        logger.LogInformation("Inicialización de base de datos completada exitosamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error crítico durante la inicialización de la base de datos: {Error}", ex.Message);
        
        // En producción, intentar continuar sin seed data
        if (app.Environment.IsProduction())
        {
            logger.LogWarning("Continuando sin seed data completo en producción...");
        }
        else
        {
            throw;
        }
    }
}

app.Run();
