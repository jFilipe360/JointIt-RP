using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoinIt.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConvitesEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConvitesEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    EmissorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecetorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondidoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConvitesEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConvitesEvento_AspNetUsers_EmissorId",
                        column: x => x.EmissorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConvitesEvento_AspNetUsers_RecetorId",
                        column: x => x.RecetorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConvitesEvento_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesEvento_EmissorId",
                table: "ConvitesEvento",
                column: "EmissorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesEvento_EventoId_RecetorId",
                table: "ConvitesEvento",
                columns: new[] { "EventoId", "RecetorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesEvento_RecetorId",
                table: "ConvitesEvento",
                column: "RecetorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConvitesEvento");
        }
    }
}
