using parcial.Models;

namespace parcial.ViewModels;

public class CatalogoCursosViewModel
{
    public FiltrosCursosViewModel Filtros { get; set; } = new();
    public List<Curso> Cursos { get; set; } = new();
    public int TotalCursos { get; set; }
    public bool FiltrosAplicados { get; set; }
}

public class DetalleCursoViewModel
{
    public Curso Curso { get; set; } = new();
    public bool UsuarioAutenticado { get; set; }
    public bool YaMatriculado { get; set; }
    public bool CupoDisponible { get; set; }
    public int MatriculasConfirmadas { get; set; }
    public string? MensajeError { get; set; }
    public string? MensajeExito { get; set; }
}