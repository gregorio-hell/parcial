namespace parcial.ViewModels;

/// <summary>
/// ViewModel para mostrar información del último curso visitado en sesión
/// </summary>
public class UltimoCursoVisitadoViewModel
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public string CodigoCurso { get; set; } = string.Empty;
}