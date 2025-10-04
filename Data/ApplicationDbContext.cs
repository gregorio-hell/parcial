using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using parcial.Models;

namespace parcial.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

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

            // Restricción: Creditos > 0
            entity.HasCheckConstraint("CK_Curso_Creditos", "[Creditos] > 0");
            
            // Restricción: HorarioInicio < HorarioFin
            entity.HasCheckConstraint("CK_Curso_Horario", "[HorarioInicio] < [HorarioFin]");
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
}