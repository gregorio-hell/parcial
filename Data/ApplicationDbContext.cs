using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using parcial.Models;
using Microsoft.AspNetCore.Identity;

namespace parcial.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

    /// <summary>
    /// Obtiene el número de estudiantes matriculados confirmados en un curso
    /// </summary>
    public int GetMatriculasConfirmadasCount(int cursoId)
    {
        return Matriculas.Count(m => m.CursoId == cursoId && m.Estado == EstadoMatricula.Confirmada);
    }

    /// <summary>
    /// Verifica si un usuario ya está matriculado en un curso específico
    /// </summary>
    public bool UsuarioYaMatriculado(int cursoId, string usuarioId)
    {
        return Matriculas.Any(m => m.CursoId == cursoId && m.UsuarioId == usuarioId);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración del modelo Curso
        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(10);
            entity.HasIndex(e => e.Codigo)
                .IsUnique();
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Creditos)
                .IsRequired();
            entity.Property(e => e.CupoMaximo)
                .IsRequired();
            entity.Property(e => e.HorarioInicio)
                .IsRequired();
            entity.Property(e => e.HorarioFin)
                .IsRequired();
            entity.Property(e => e.Activo)
                .IsRequired();

            // Restricciones usando ToTable
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Curso_Creditos", "[Creditos] > 0");
                t.HasCheckConstraint("CK_Curso_Horario", "[HorarioInicio] < [HorarioFin]");
            });
        });

        // Configuración del modelo Matricula
        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CursoId)
                .IsRequired();
            entity.Property(e => e.UsuarioId)
                .IsRequired();
            entity.Property(e => e.FechaRegistro)
                .IsRequired();
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasConversion<string>();

            // Restricción única: Un usuario no puede estar matriculado más de una vez en el mismo curso
            entity.HasIndex(e => new { e.CursoId, e.UsuarioId })
                .IsUnique()
                .HasDatabaseName("IX_Matricula_Curso_Usuario_Unique");

            // Relación con Curso
            entity.HasOne(e => e.Curso)
                .WithMany()
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Usuario (Identity)
            entity.HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configura los datos iniciales (seed data)
    /// </summary>
    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Crear roles si no existen
        if (!await roleManager.RoleExistsAsync("Coordinador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Coordinador"));
        }

        if (!await roleManager.RoleExistsAsync("Estudiante"))
        {
            await roleManager.CreateAsync(new IdentityRole("Estudiante"));
        }

        // Crear usuario coordinador si no existe
        var coordinadorEmail = "coordinador@universidad.edu";
        var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
        
        if (coordinador == null)
        {
            coordinador = new IdentityUser
            {
                UserName = coordinadorEmail,
                Email = coordinadorEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(coordinador, "Coordinador123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinador, "Coordinador");
            }
        }

        // Crear cursos iniciales si no existen
        if (!context.Cursos.Any())
        {
            var cursosIniciales = new List<Curso>
            {
                new Curso
                {
                    Codigo = "CS101",
                    Nombre = "Introducción a la Programación",
                    Creditos = 3,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeOnly(8, 0),
                    HorarioFin = new TimeOnly(10, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS201",
                    Nombre = "Estructuras de Datos y Algoritmos",
                    Creditos = 4,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeOnly(10, 30),
                    HorarioFin = new TimeOnly(12, 30),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS301",
                    Nombre = "Bases de Datos",
                    Creditos = 3,
                    CupoMaximo = 20,
                    HorarioInicio = new TimeOnly(14, 0),
                    HorarioFin = new TimeOnly(16, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS401",
                    Nombre = "Ingeniería de Software",
                    Creditos = 4,
                    CupoMaximo = 15,
                    HorarioInicio = new TimeOnly(16, 30),
                    HorarioFin = new TimeOnly(18, 30),
                    Activo = true
                }
            };

            context.Cursos.AddRange(cursosIniciales);
            await context.SaveChangesAsync();
        }
    }
}