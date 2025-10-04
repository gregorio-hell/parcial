using parcial.Data;
using parcial.Models;
using Microsoft.EntityFrameworkCore;

namespace parcial.Services;

public interface IMatriculaService
{
    Task<bool> PuedeMatricularseAsync(int cursoId, string usuarioId);
    Task<string> ValidarMatriculaAsync(int cursoId, string usuarioId);
    Task<bool> MatricularEstudianteAsync(int cursoId, string usuarioId);
}

public class MatriculaService : IMatriculaService
{
    private readonly ApplicationDbContext _context;

    public MatriculaService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica si un estudiante puede matricularse en un curso
    /// </summary>
    public async Task<bool> PuedeMatricularseAsync(int cursoId, string usuarioId)
    {
        var validacion = await ValidarMatriculaAsync(cursoId, usuarioId);
        return string.IsNullOrEmpty(validacion);
    }

    /// <summary>
    /// Valida las reglas de negocio para la matrícula y retorna el mensaje de error si hay alguno
    /// </summary>
    public async Task<string> ValidarMatriculaAsync(int cursoId, string usuarioId)
    {
        // Verificar que el curso existe y está activo
        var curso = await _context.Cursos.FindAsync(cursoId);
        if (curso == null)
        {
            return "El curso no existe.";
        }

        if (!curso.Activo)
        {
            return "El curso no está activo.";
        }

        // Verificar que el usuario no esté ya matriculado en el curso
        var yaMatriculado = await _context.Matriculas
            .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == usuarioId);

        if (yaMatriculado)
        {
            return "Ya estás matriculado en este curso.";
        }

        // Verificar que no se exceda el cupo máximo
        var matriculasConfirmadas = await _context.Matriculas
            .CountAsync(m => m.CursoId == cursoId && m.Estado == EstadoMatricula.Confirmada);

        if (matriculasConfirmadas >= curso.CupoMaximo)
        {
            return "El curso ha alcanzado su cupo máximo.";
        }

        return string.Empty; // No hay errores
    }

    /// <summary>
    /// Matricula un estudiante en un curso después de validar las reglas de negocio
    /// </summary>
    public async Task<bool> MatricularEstudianteAsync(int cursoId, string usuarioId)
    {
        var validacion = await ValidarMatriculaAsync(cursoId, usuarioId);
        if (!string.IsNullOrEmpty(validacion))
        {
            return false;
        }

        var matricula = new Matricula
        {
            CursoId = cursoId,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            Estado = EstadoMatricula.Pendiente
        };

        _context.Matriculas.Add(matricula);
        
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // Esto podría ocurrir si hay una violación de la restricción única
            return false;
        }
    }
}