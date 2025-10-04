using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace parcial.Models;

public class Matricula
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El curso es obligatorio")]
    [Display(Name = "Curso")]
    public int CursoId { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio")]
    [Display(Name = "Usuario")]
    public string UsuarioId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de registro es obligatoria")]
    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El estado es obligatorio")]
    [Display(Name = "Estado")]
    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

    // Propiedades de navegación
    public virtual Curso? Curso { get; set; }
    public virtual IdentityUser? Usuario { get; set; }
}