using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging básico
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configurar base de datos con fallback
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                      ?? "DataSource=app.db;Cache=Shared";

Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Identity MUY básico - sin roles por ahora
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC básico
builder.Services.AddControllersWithViews();

// Cache en memoria solamente
builder.Services.AddDistributedMemoryCache();

// Servicios básicos - comentados por ahora
// builder.Services.AddScoped<IMatriculaService, MatriculaService>();
// builder.Services.AddScoped<ISesionService, SesionService>();
// builder.Services.AddScoped<ICursosCache, CursosCache>();

var app = builder.Build();

// Inicializar base de datos de forma MUY simple
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("Ensuring database is created...");
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("Database ready!");
}
catch (Exception ex)
{
    Console.WriteLine($"Database setup error: {ex.Message}");
}

// Pipeline ultra simple
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

Console.WriteLine("App starting...");
app.Run();
