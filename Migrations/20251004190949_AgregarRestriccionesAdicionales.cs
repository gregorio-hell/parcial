using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace parcial.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRestriccionesAdicionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matriculas_CursoId",
                table: "Matriculas");

            migrationBuilder.CreateIndex(
                name: "IX_Matricula_Curso_Usuario_Unique",
                table: "Matriculas",
                columns: new[] { "CursoId", "UsuarioId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matricula_Curso_Usuario_Unique",
                table: "Matriculas");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_CursoId",
                table: "Matriculas",
                column: "CursoId");
        }
    }
}
