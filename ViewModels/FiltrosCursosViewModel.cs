using System.ComponentModel.DataAnnotations;

namespace parcial.ViewModels;

public class FiltrosCursosViewModel : IValidatableObject
{
    [Display(Name = "Nombre del curso")]
    public string? Nombre { get; set; }

    [Display(Name = "Créditos mínimos")]
    [Range(0, int.MaxValue, ErrorMessage = "Los créditos no pueden ser negativos")]
    public int? CreditosMin { get; set; }

    [Display(Name = "Créditos máximos")]
    [Range(0, int.MaxValue, ErrorMessage = "Los créditos no pueden ser negativos")]
    public int? CreditosMax { get; set; }

    [Display(Name = "Horario desde")]
    [DataType(DataType.Time)]
    public TimeOnly? HorarioDesde { get; set; }

    [Display(Name = "Horario hasta")]
    [DataType(DataType.Time)]
    public TimeOnly? HorarioHasta { get; set; }

    /// <summary>
    /// Validación personalizada para asegurar que CreditosMax >= CreditosMin y HorarioHasta > HorarioDesde
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreditosMin.HasValue && CreditosMax.HasValue && CreditosMin > CreditosMax)
        {
            yield return new ValidationResult(
                "Los créditos máximos deben ser mayores o iguales a los créditos mínimos",
                new[] { nameof(CreditosMax) });
        }

        if (HorarioDesde.HasValue && HorarioHasta.HasValue && HorarioDesde >= HorarioHasta)
        {
            yield return new ValidationResult(
                "El horario hasta debe ser posterior al horario desde",
                new[] { nameof(HorarioHasta) });
        }
    }
}