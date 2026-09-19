using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoinIt.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensagensEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EnviadaEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensagensEvento_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensagensEvento_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensEvento_EventoId_EnviadaEm",
                table: "MensagensEvento",
                columns: new[] { "EventoId", "EnviadaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensEvento_UtilizadorId",
                table: "MensagensEvento",
                column: "UtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensagensEvento");
        }
    }
}
