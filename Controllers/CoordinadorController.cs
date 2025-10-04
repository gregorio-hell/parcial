using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;
using parcial.Services;

namespace parcial.Controllers;

/// <summary>
/// Controlador para el panel de coordinador con CRUD de cursos y gestión de matrículas
/// </summary>
[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICursosCache _cursosCache;

    public CoordinadorController(ApplicationDbContext context, ICursosCache cursosCache)
    {
        _context = context;
        _cursosCache = cursosCache;
    }

    /// <summary>
    /// Panel principal del coordinador
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos
            .OrderBy(c => c.Codigo)
            .ToListAsync();

        return View(cursos);
    }

    /// <summary>
    /// Vista para crear un nuevo curso
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Crear un nuevo curso
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        if (ModelState.IsValid)
        {
            // Verificar que no existe un curso con el mismo código
            var cursoExistente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Codigo == curso.Codigo);

            if (cursoExistente != null)
            {
                ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                return View(curso);
            }

            _context.Add(curso);
            await _context.SaveChangesAsync();

            // Invalidar cache
            await _cursosCache.InvalidarCacheAsync();

            TempData["SuccessMessage"] = "Curso creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    /// <summary>
    /// Vista para editar un curso
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null)
        {
            return NotFound();
        }
        return View(curso);
    }

    /// <summary>
    /// Editar un curso existente
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        if (id != curso.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Verificar que no existe otro curso con el mismo código
            var cursoExistente = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Codigo == curso.Codigo && c.Id != curso.Id);

            if (cursoExistente != null)
            {
                ModelState.AddModelError("Codigo", "Ya existe otro curso con este código.");
                return View(curso);
            }

            try
            {
                _context.Update(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache
                await _cursosCache.InvalidarCacheAsync();

                TempData["SuccessMessage"] = "Curso actualizado exitosamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(curso.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    /// <summary>
    /// Desactivar un curso
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso != null)
        {
            curso.Activo = false;
            await _context.SaveChangesAsync();

            // Invalidar cache
            await _cursosCache.InvalidarCacheAsync();

            TempData["SuccessMessage"] = "Curso desactivado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Activar un curso
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso != null)
        {
            curso.Activo = true;
            await _context.SaveChangesAsync();

            // Invalidar cache
            await _cursosCache.InvalidarCacheAsync();

            TempData["SuccessMessage"] = "Curso activado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Ver matrículas de un curso específico
    /// </summary>
    public async Task<IActionResult> Matriculas(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos
            .Include(c => c.Matriculas)
            .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
        {
            return NotFound();
        }

        return View(curso);
    }

    /// <summary>
    /// Confirmar una matrícula
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarMatricula(int id, int cursoId)
    {
        var matricula = await _context.Matriculas
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matricula != null)
        {
            matricula.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Matrícula confirmada exitosamente.";
        }

        return RedirectToAction(nameof(Matriculas), new { id = cursoId });
    }

    /// <summary>
    /// Cancelar una matrícula
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarMatricula(int id, int cursoId)
    {
        var matricula = await _context.Matriculas
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matricula != null)
        {
            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Matrícula cancelada exitosamente.";
        }

        return RedirectToAction(nameof(Matriculas), new { id = cursoId });
    }

    private bool CursoExists(int id)
    {
        return _context.Cursos.Any(e => e.Id == id);
    }
}