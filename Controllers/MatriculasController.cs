using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using parcial.Data;
using parcial.Models;
using System.Security.Claims;

namespace parcial.Controllers;

public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatriculasController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista las matrículas del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MisMatriculas()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(usuarioId))
        {
            // Para usuarios no autenticados, devolvemos una lista vacía
            // La vista se encargará de mostrar el mensaje de login
            return View(new List<Matricula>());
        }

        var matriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaRegistro)
            .ToListAsync();

        return View(matriculas);
    }

    /// <summary>
    /// Cancela una matrícula en estado Pendiente
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Cancelar(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var matricula = await _context.Matriculas
            .Include(m => m.Curso)
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == usuarioId);

        if (matricula == null)
        {
            TempData["MensajeError"] = "No se encontró la matrícula o no tienes permisos para cancelarla.";
            return RedirectToAction("MisMatriculas");
        }

        if (matricula.Estado != EstadoMatricula.Pendiente)
        {
            TempData["MensajeError"] = "Solo se pueden cancelar matrículas en estado Pendiente.";
            return RedirectToAction("MisMatriculas");
        }

        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();

        TempData["MensajeExito"] = $"La matrícula del curso '{matricula.Curso?.Codigo}' ha sido cancelada exitosamente.";
        return RedirectToAction("MisMatriculas");
    }
}