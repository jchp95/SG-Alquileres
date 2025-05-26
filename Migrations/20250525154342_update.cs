using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                table: "tb_inmueble");

            migrationBuilder.AlterColumn<bool>(
                name: "factivo",
                table: "tb_inmueble",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTime>(
                name: "ffechaRegistro",
                table: "tb_inmueble",
                type: "datetime2",
                unicode: false,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                table: "tb_inmueble",
                column: "fkid_propietario",
                principalTable: "tb_propietario",
                principalColumn: "fid_propietario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                table: "tb_inmueble");

            migrationBuilder.DropColumn(
                name: "ffechaRegistro",
                table: "tb_inmueble");

            migrationBuilder.AlterColumn<bool>(
                name: "factivo",
                table: "tb_inmueble",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                table: "tb_inmueble",
                column: "fkid_propietario",
                principalTable: "tb_propietario",
                principalColumn: "fid_propietario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
