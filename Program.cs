using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using parcial.Data;
using parcial.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging básico
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configurar base de datos con fallback
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                      ?? "DataSource=app.db;Cache=Shared";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Identity básico
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC
builder.Services.AddControllersWithViews();

// Servicios básicos
builder.Services.AddScoped<IMatriculaService, MatriculaService>();
builder.Services.AddDistributedMemoryCache(); // Solo memoria, sin Redis
builder.Services.AddScoped<ISesionService, SesionService>();
builder.Services.AddScoped<ICursosCache, CursosCache>();

var app = builder.Build();

// Pipeline simple
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// No HTTPS redirect en contenedores
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
