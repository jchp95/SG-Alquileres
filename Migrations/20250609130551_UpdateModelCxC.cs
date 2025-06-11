using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelCxC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "fstatus",
                table: "tb_cxc",
                type: "char(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldMaxLength: 450);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "fstatus",
                table: "tb_cxc",
                type: "bit",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)",
                oldMaxLength: 1);
        }
    }
}
