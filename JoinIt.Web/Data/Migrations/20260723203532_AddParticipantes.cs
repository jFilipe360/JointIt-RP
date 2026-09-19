using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoinIt.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Participantes",
                columns: table => new
                {
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    DataPedido = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participantes", x => new { x.EventoId, x.UtilizadorId });
                    table.ForeignKey(
                        name: "FK_Participantes_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participantes_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Participantes_UtilizadorId",
                table: "Participantes",
                column: "UtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participantes");
        }
    }
}
