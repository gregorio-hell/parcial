using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;
using System.Text.Json;

namespace parcial.Services;

public interface ICursosCache
{
    Task<List<Curso>> ObtenerCursosActivosAsync();
    Task InvalidarCacheAsync();
    Task<Curso?> ObtenerCursoPorIdAsync(int id);
}

public class CursosCache : ICursosCache
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CursosCache> _logger;
    private const string CURSOS_ACTIVOS_KEY = "cursos_activos";
    private const string CURSO_DETALLE_KEY = "curso_detalle";
    private const int CACHE_DURATION_SECONDS = 60;

    public CursosCache(
        ApplicationDbContext context, 
        IDistributedCache cache,
        ILogger<CursosCache> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Curso>> ObtenerCursosActivosAsync()
    {
        var cacheKey = CURSOS_ACTIVOS_KEY;
        var cursosJson = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cursosJson))
        {
            _logger.LogInformation("Cursos obtenidos desde cache Redis");
            try
            {
                var cursos = JsonSerializer.Deserialize<List<Curso>>(cursosJson);
                return cursos ?? new List<Curso>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Error deserializando cursos desde cache. Obteniendo desde BD.");
            }
        }

        // Si no está en cache o hay error, obtener desde BD
        _logger.LogInformation("Cursos obtenidos desde base de datos");
        var cursosFromDb = await _context.Cursos
            .Where(c => c.Activo)
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        // Guardar en cache
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_DURATION_SECONDS)
        };

        var jsonToCache = JsonSerializer.Serialize(cursosFromDb);
        await _cache.SetStringAsync(cacheKey, jsonToCache, options);
        _logger.LogInformation($"Cursos guardados en cache Redis por {CACHE_DURATION_SECONDS} segundos");

        return cursosFromDb;
    }

    public async Task<Curso?> ObtenerCursoPorIdAsync(int id)
    {
        var cacheKey = $"{CURSO_DETALLE_KEY}:{id}";
        var cursoJson = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cursoJson))
        {
            _logger.LogInformation($"Curso {id} obtenido desde cache Redis");
            try
            {
                return JsonSerializer.Deserialize<Curso>(cursoJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, $"Error deserializando curso {id} desde cache. Obteniendo desde BD.");
            }
        }

        // Si no está en cache o hay error, obtener desde BD
        _logger.LogInformation($"Curso {id} obtenido desde base de datos");
        var cursoFromDb = await _context.Cursos.FindAsync(id);

        if (cursoFromDb != null)
        {
            // Guardar en cache solo si el curso existe y está activo
            if (cursoFromDb.Activo)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_DURATION_SECONDS)
                };

                var jsonToCache = JsonSerializer.Serialize(cursoFromDb);
                await _cache.SetStringAsync(cacheKey, jsonToCache, options);
                _logger.LogInformation($"Curso {id} guardado en cache Redis por {CACHE_DURATION_SECONDS} segundos");
            }
        }

        return cursoFromDb;
    }

    public async Task InvalidarCacheAsync()
    {
        _logger.LogInformation("Invalidando cache de cursos");
        
        // Invalidar cache de cursos activos
        await _cache.RemoveAsync(CURSOS_ACTIVOS_KEY);
        
        // Para invalidar todos los cursos individuales, necesitaríamos un patrón más complejo
        // Por simplicidad, solo invalidamos el listado principal
        // En un sistema más robusto, podríamos usar Redis pattern matching o tags
        
        _logger.LogInformation("Cache de cursos invalidado exitosamente");
    }
}