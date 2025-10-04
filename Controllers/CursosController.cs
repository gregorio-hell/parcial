using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;
using parcial.ViewModels;
using parcial.Services;
using System.Security.Claims;

namespace parcial.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMatriculaService _matriculaService;
    private readonly ISesionService _sesionService;
    private readonly ICursosCache _cursosCache;

    public CursosController(
        ApplicationDbContext context, 
        UserManager<IdentityUser> userManager,
        IMatriculaService matriculaService,
        ISesionService sesionService,
        ICursosCache cursosCache)
    {
        _context = context;
        _userManager = userManager;
        _matriculaService = matriculaService;
        _sesionService = sesionService;
        _cursosCache = cursosCache;
    }

    /// <summary>
    /// Vista del catálogo de cursos con filtros
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Catalogo(FiltrosCursosViewModel? filtros)
    {
        // Si no se proporcionan filtros, crear un objeto vacío
        filtros ??= new FiltrosCursosViewModel();

        // Validar el modelo de filtros
        if (!TryValidateModel(filtros))
        {
            var viewModel = new CatalogoCursosViewModel
            {
                Filtros = filtros,
                Cursos = new List<Curso>(),
                TotalCursos = 0,
                FiltrosAplicados = false
            };
            return View(viewModel);
        }

        // Obtener cursos desde cache (si no hay filtros) o desde BD (si hay filtros)
        List<Curso> cursos;
        bool filtrosAplicados = false;

        // Verificar si hay filtros aplicados
        bool hayFiltros = !string.IsNullOrWhiteSpace(filtros.Nombre) ||
                         filtros.CreditosMin.HasValue ||
                         filtros.CreditosMax.HasValue ||
                         filtros.HorarioDesde.HasValue ||
                         filtros.HorarioHasta.HasValue;

        if (!hayFiltros)
        {
            // Sin filtros: usar cache
            cursos = await _cursosCache.ObtenerCursosActivosAsync();
        }
        else
        {
            // Con filtros: consultar BD directamente
            filtrosAplicados = true;
            var query = _context.Cursos.Where(c => c.Activo);

            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                query = query.Where(c => c.Nombre.Contains(filtros.Nombre) || c.Codigo.Contains(filtros.Nombre));
            }

            if (filtros.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            }

            if (filtros.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            }

            if (filtros.HorarioDesde.HasValue)
            {
                query = query.Where(c => c.HorarioInicio >= filtros.HorarioDesde.Value);
            }

            if (filtros.HorarioHasta.HasValue)
            {
                query = query.Where(c => c.HorarioFin <= filtros.HorarioHasta.Value);
            }

            // Ordenar por código de curso
            query = query.OrderBy(c => c.Codigo);

            // Ejecutar query
            cursos = await query.ToListAsync();
        }

        var catalogoViewModel = new CatalogoCursosViewModel
        {
            Filtros = filtros,
            Cursos = cursos,
            TotalCursos = cursos.Count,
            FiltrosAplicados = filtrosAplicados
        };

        return View(catalogoViewModel);
    }

    /// <summary>
    /// Vista de detalle de un curso específico
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        
        if (curso == null || !curso.Activo)
        {
            return NotFound("El curso no existe o no está activo.");
        }

        // Guardar último curso visitado en sesión
        await _sesionService.GuardarUltimoCursoVisitadoAsync(id, curso.Codigo, curso.Nombre);

        var usuarioAutenticado = User.Identity?.IsAuthenticated == true;
        var yaMatriculado = false;
        var matriculasConfirmadas = await _context.Matriculas
            .CountAsync(m => m.CursoId == id && m.Estado == EstadoMatricula.Confirmada);

        if (usuarioAutenticado)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == id && m.UsuarioId == usuarioId);
        }

        var viewModel = new DetalleCursoViewModel
        {
            Curso = curso,
            UsuarioAutenticado = usuarioAutenticado,
            YaMatriculado = yaMatriculado,
            CupoDisponible = matriculasConfirmadas < curso.CupoMaximo,
            MatriculasConfirmadas = matriculasConfirmadas
        };

        return View(viewModel);
    }

    /// <summary>
    /// Acción para inscribirse en un curso
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscribirse(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(usuarioId))
        {
            TempData["MensajeError"] = "Debes estar autenticado para inscribirte en un curso.";
            return RedirectToAction("Detalle", new { id });
        }

        try
        {
            // Validar reglas de negocio antes de procesar la inscripción
            var mensajeValidacion = await _matriculaService.ValidarMatriculaAsync(id, usuarioId);
            
            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                TempData["MensajeError"] = mensajeValidacion;
                return RedirectToAction("Detalle", new { id });
            }

            // Procesar la inscripción
            var resultado = await _matriculaService.MatricularEstudianteAsync(id, usuarioId);

            if (resultado)
            {
                TempData["MensajeExito"] = "¡Inscripción exitosa! Tu matrícula ha sido registrada en estado PENDIENTE. " +
                                         "Un coordinador académico revisará y confirmará tu inscripción.";
            }
            else
            {
                TempData["MensajeError"] = "No se pudo procesar la inscripción. Es posible que ya estés matriculado " +
                                         "o que se haya alcanzado el cupo máximo. Por favor, inténtalo nuevamente.";
            }
        }
        catch (Exception)
        {
            TempData["MensajeError"] = "Ocurrió un error inesperado durante la inscripción. " +
                                     "Por favor, contacta al administrador del sistema.";
            
            // Log del error para debugging (en un sistema real)
            // _logger.LogError(ex, "Error durante inscripción del usuario {UserId} en curso {CursoId}", usuarioId, id);
        }

        return RedirectToAction("Detalle", new { id });
    }

    /// <summary>
    /// Acción para limpiar todos los filtros
    /// </summary>
    [HttpGet]
    public IActionResult LimpiarFiltros()
    {
        return RedirectToAction("Catalogo");
    }
}