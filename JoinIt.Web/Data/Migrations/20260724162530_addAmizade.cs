using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoinIt.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class addAmizade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amizades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmissorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecetorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amizades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amizades_AspNetUsers_EmissorId",
                        column: x => x.EmissorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Amizades_AspNetUsers_RecetorId",
                        column: x => x.RecetorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amizades_EmissorId_RecetorId",
                table: "Amizades",
                columns: new[] { "EmissorId", "RecetorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Amizades_RecetorId",
                table: "Amizades",
                column: "RecetorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amizades");
        }
    }
}
