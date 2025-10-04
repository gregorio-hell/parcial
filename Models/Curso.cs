using System.ComponentModel.DataAnnotations;

namespace parcial.Models;

public class Curso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(10, ErrorMessage = "El código no puede superar los 10 caracteres")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los créditos son obligatorios")]
    [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayor a 0")]
    [Display(Name = "Créditos")]
    public int Creditos { get; set; }

    [Required(ErrorMessage = "El cupo máximo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El cupo máximo debe ser mayor a 0")]
    [Display(Name = "Cupo Máximo")]
    public int CupoMaximo { get; set; }

    [Required(ErrorMessage = "El horario de inicio es obligatorio")]
    [Display(Name = "Horario de Inicio")]
    public TimeOnly HorarioInicio { get; set; }

    [Required(ErrorMessage = "El horario de fin es obligatorio")]
    [Display(Name = "Horario de Fin")]
    public TimeOnly HorarioFin { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Propiedades de navegación
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    // Validación personalizada para asegurar que HorarioInicio < HorarioFin
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HorarioInicio >= HorarioFin)
        {
            yield return new ValidationResult(
                "El horario de inicio debe ser menor al horario de fin",
                new[] { nameof(HorarioInicio), nameof(HorarioFin) });
        }
    }
}