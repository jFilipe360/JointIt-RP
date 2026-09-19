using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoinIt.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primeiro permite valores nulos para os eventos já existentes.
            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Eventos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Local",
                table: "Eventos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "Eventos",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            // Nos eventos antigos, define o fim duas horas após o início.
            migrationBuilder.Sql(
                """
                UPDATE [Eventos]
                SET [DataFim] = DATEADD(HOUR, 2, [DataHora])
                WHERE [DataFim] IS NULL;
                """);

            // Nos eventos antigos, coloca um local temporário.
            migrationBuilder.Sql(
                """
                UPDATE [Eventos]
                SET [Local] = N'Local não definido'
                WHERE [Local] IS NULL
                   OR LTRIM(RTRIM([Local])) = N'';
                """);

            // Depois dos dados estarem preenchidos, torna os campos obrigatórios.
            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFim",
                table: "Eventos",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Local",
                table: "Eventos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Local",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "Eventos");
        }
    }
}