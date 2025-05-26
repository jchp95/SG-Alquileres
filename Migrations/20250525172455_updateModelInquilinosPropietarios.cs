using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class updateModelInquilinosPropietarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FfechaRegistro",
                table: "tb_propietario",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FfechaRegistro",
                table: "tb_inquilino",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FfechaRegistro",
                table: "tb_propietario");

            migrationBuilder.DropColumn(
                name: "FfechaRegistro",
                table: "tb_inquilino");
        }
    }
}
