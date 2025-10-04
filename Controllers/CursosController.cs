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

    public CursosController(
        ApplicationDbContext context, 
        UserManager<IdentityUser> userManager,
        IMatriculaService matriculaService)
    {
        _context = context;
        _userManager = userManager;
        _matriculaService = matriculaService;
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

        // Construir query base - solo cursos activos
        var query = _context.Cursos.Where(c => c.Activo);

        // Aplicar filtros
        bool filtrosAplicados = false;

        if (!string.IsNullOrWhiteSpace(filtros.Nombre))
        {
            query = query.Where(c => c.Nombre.Contains(filtros.Nombre) || c.Codigo.Contains(filtros.Nombre));
            filtrosAplicados = true;
        }

        if (filtros.CreditosMin.HasValue)
        {
            query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            filtrosAplicados = true;
        }

        if (filtros.CreditosMax.HasValue)
        {
            query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            filtrosAplicados = true;
        }

        if (filtros.HorarioDesde.HasValue)
        {
            query = query.Where(c => c.HorarioInicio >= filtros.HorarioDesde.Value);
            filtrosAplicados = true;
        }

        if (filtros.HorarioHasta.HasValue)
        {
            query = query.Where(c => c.HorarioFin <= filtros.HorarioHasta.Value);
            filtrosAplicados = true;
        }

        // Ordenar por código de curso
        query = query.OrderBy(c => c.Codigo);

        // Ejecutar query
        var cursos = await query.ToListAsync();

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
            return RedirectToAction("Login", "Account");
        }

        // Usar el servicio de matrícula para validar y procesar la inscripción
        var resultado = await _matriculaService.MatricularEstudianteAsync(id, usuarioId);
        var mensajeValidacion = await _matriculaService.ValidarMatriculaAsync(id, usuarioId);

        if (resultado)
        {
            TempData["MensajeExito"] = "Te has inscrito exitosamente en el curso. Tu matrícula está pendiente de confirmación.";
        }
        else
        {
            TempData["MensajeError"] = !string.IsNullOrEmpty(mensajeValidacion) 
                ? mensajeValidacion 
                : "No se pudo procesar la inscripción. Inténtalo nuevamente.";
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