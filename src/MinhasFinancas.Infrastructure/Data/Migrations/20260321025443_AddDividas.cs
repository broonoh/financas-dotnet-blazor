using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDividas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dividas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_devedor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    quantidade_parcelas = table.Column<int>(type: "integer", nullable: false),
                    data_compra = table.Column<DateOnly>(type: "date", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dividas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parcelas_divida",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    divida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    paga = table.Column<bool>(type: "boolean", nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcelas_divida", x => x.id);
                    table.ForeignKey(
                        name: "FK_parcelas_divida_dividas_divida_id",
                        column: x => x.divida_id,
                        principalTable: "dividas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_dividas_usuario",
                table: "dividas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_parcelas_divida_divida_id",
                table: "parcelas_divida",
                column: "divida_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parcelas_divida");

            migrationBuilder.DropTable(
                name: "dividas");
        }
    }
}
