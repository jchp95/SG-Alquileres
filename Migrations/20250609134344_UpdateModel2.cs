using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FfechaCancelacion",
                table: "tb_cxc",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FmotivoCancelacion",
                table: "tb_cxc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FfechaCancelacion",
                table: "tb_cxc");

            migrationBuilder.DropColumn(
                name: "FmotivoCancelacion",
                table: "tb_cxc");
        }
    }
}
