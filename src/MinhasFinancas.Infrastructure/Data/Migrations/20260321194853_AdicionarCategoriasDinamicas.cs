using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriasDinamicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_despesa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_despesa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias_receita",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_receita", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_categorias_despesa_usuario_nome",
                table: "categorias_despesa",
                columns: new[] { "usuario_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_categorias_receita_usuario_nome",
                table: "categorias_receita",
                columns: new[] { "usuario_id", "nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categorias_despesa");

            migrationBuilder.DropTable(
                name: "categorias_receita");
        }
    }
}
