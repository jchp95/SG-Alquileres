using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fstatus",
                table: "tb_cxc",
                newName: "fstatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fstatus",
                table: "tb_cxc",
                newName: "Fstatus");
        }
    }
}
