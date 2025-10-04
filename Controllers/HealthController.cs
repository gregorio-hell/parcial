using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using parcial.Data;

namespace parcial.Controllers
{
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;
        private readonly IWebHostEnvironment _env;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            var healthInfo = new
            {
                Status = "Healthy",
                Environment = _env.EnvironmentName,
                Timestamp = DateTime.UtcNow,
                Application = GetApplicationInfo(),
                Database = "Checking..."
            };

            return Json(healthInfo);
        }

        public async Task<IActionResult> Full()
        {
            var healthInfo = new
            {
                Status = "Healthy",
                Environment = _env.EnvironmentName,
                Timestamp = DateTime.UtcNow,
                Database = await CheckDatabase(),
                Application = GetApplicationInfo()
            };

            return Json(healthInfo);
        }

        private async Task<object> CheckDatabase()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                var cursosCount = await _context.Cursos.CountAsync();
                var usersCount = await _context.Users.CountAsync();
                
                return new
                {
                    Status = "Connected",
                    CursosCount = cursosCount,
                    UsersCount = usersCount,
                    ConnectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "..."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la base de datos");
                return new
                {
                    Status = "Error",
                    Message = ex.Message,
                    Type = ex.GetType().Name
                };
            }
        }

        private object GetApplicationInfo()
        {
            return new
            {
                Version = "1.0.0",
                Runtime = Environment.Version.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingDirectory = Environment.CurrentDirectory,
                Environment = Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
                    .Where(e => e.Key.ToString()?.StartsWith("ASPNETCORE") == true || e.Key.ToString()?.Contains("CONNECTION") == true)
                    .ToDictionary(e => e.Key, e => e.Value)
            };
        }
    }
}