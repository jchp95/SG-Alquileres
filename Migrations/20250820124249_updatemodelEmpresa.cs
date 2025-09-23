using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class updatemodelEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_configuracion");

            migrationBuilder.AddColumn<bool>(
                name: "factivar_cobro_rapido",
                table: "tb_empresa",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "factivar_cobro_rapido",
                table: "tb_empresa");

            migrationBuilder.CreateTable(
                name: "tb_configuracion",
                columns: table => new
                {
                    fid_config = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ftipo_cobro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_configuracion", x => x.fid_config);
                });

            migrationBuilder.InsertData(
                table: "tb_configuracion",
                columns: new[] { "fid_config", "ftipo_cobro" },
                values: new object[,]
                {
                    { 1, "Detallado" },
                    { 2, "Rapdio" }
                });
        }
    }
}
