using Microsoft.Extensions.Caching.Distributed;
using parcial.Models;
using System.Text.Json;

namespace parcial.Services;

public interface ISesionService
{
    Task GuardarUltimoCursoVisitadoAsync(int cursoId, string nombreCurso, string codigoCurso);
    Task<(int Id, string Nombre, string Codigo)?> ObtenerUltimoCursoVisitadoAsync();
    Task LimpiarUltimoCursoVisitadoAsync();
}

public class SesionService : ISesionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _cache;
    private const string ULTIMO_CURSO_KEY = "ultimo_curso_visitado";

    public SesionService(IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public async Task GuardarUltimoCursoVisitadoAsync(int cursoId, string nombreCurso, string codigoCurso)
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId)) return;

        var cursoInfo = new
        {
            Id = cursoId,
            Nombre = nombreCurso,
            Codigo = codigoCurso,
            FechaVisita = DateTime.Now
        };

        var json = JsonSerializer.Serialize(cursoInfo);
        var cacheKey = $"{ULTIMO_CURSO_KEY}:{sessionId}";
        
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30) // La sesión expira en 30 minutos sin actividad
        };

        await _cache.SetStringAsync(cacheKey, json, options);
    }

    public async Task<(int Id, string Nombre, string Codigo)?> ObtenerUltimoCursoVisitadoAsync()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId)) return null;

        var cacheKey = $"{ULTIMO_CURSO_KEY}:{sessionId}";
        var json = await _cache.GetStringAsync(cacheKey);
        
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var cursoInfo = JsonSerializer.Deserialize<dynamic>(json);
            var element = (JsonElement)cursoInfo;
            
            return (
                element.GetProperty("Id").GetInt32(),
                element.GetProperty("Nombre").GetString() ?? "",
                element.GetProperty("Codigo").GetString() ?? ""
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task LimpiarUltimoCursoVisitadoAsync()
    {
        var sessionId = GetSessionId();
        if (string.IsNullOrEmpty(sessionId)) return;

        var cacheKey = $"{ULTIMO_CURSO_KEY}:{sessionId}";
        await _cache.RemoveAsync(cacheKey);
    }

    private string? GetSessionId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session == null) return null;

        // Crear o obtener el ID de sesión
        if (string.IsNullOrEmpty(context.Session.Id))
        {
            context.Session.SetString("_init", "true");
        }

        return context.Session.Id;
    }
}