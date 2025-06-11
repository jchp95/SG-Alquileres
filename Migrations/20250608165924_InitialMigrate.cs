using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "periodos_pago",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    dias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_pago", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_auditoria",
                columns: table => new
                {
                    fid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ftabla = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fkid_registro = table.Column<int>(type: "int", nullable: false),
                    ffecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fhora = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    faccion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_auditoria", x => x.fid);
                });

            migrationBuilder.CreateTable(
                name: "tb_empresa",
                columns: table => new
                {
                    fid_empresa = table.Column<int>(type: "int", nullable: false),
                    frnc = table.Column<string>(type: "varchar(18)", unicode: false, maxLength: 18, nullable: true),
                    fnombre = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    fdireccion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    ftelefonos = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: true),
                    festlogan = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    fmensaje = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    flogo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ffondo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    fcodigoqr_web = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    fcodigoqr_redes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    femail = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    fcontraseña = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_empresa", x => x.fid_empresa);
                });

            migrationBuilder.CreateTable(
                name: "tb_propietario",
                columns: table => new
                {
                    fid_propietario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fnombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fapellidos = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fcedula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    ftelefono = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    fcelular = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false),
                    FfechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_propietario", x => x.fid_propietario);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_usuario",
                columns: table => new
                {
                    fid_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fnombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fusuario = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fnivel = table.Column<int>(type: "int", nullable: false),
                    fpassword = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    factivado = table.Column<bool>(type: "bit", nullable: false),
                    FkidUsuario = table.Column<int>(type: "int", nullable: false),
                    fkid_sucursal = table.Column<int>(type: "int", nullable: false),
                    festado_sync = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "A"),
                    factivo = table.Column<bool>(type: "bit", nullable: false),
                    identity_id = table.Column<string>(type: "nvarchar(450)", unicode: false, maxLength: 450, nullable: true),
                    tutorial_visto = table.Column<bool>(type: "bit", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_usuario", x => x.fid_usuario);
                    table.ForeignKey(
                        name: "FK_tb_usuario_AspNetUsers_identity_id",
                        column: x => x.identity_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_inmueble",
                columns: table => new
                {
                    fid_inmueble = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_propietario = table.Column<int>(type: "int", nullable: false),
                    fdescripcion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    fubicacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fprecio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ffechaRegistro = table.Column<DateTime>(type: "datetime2", unicode: false, nullable: false),
                    FkidUsuario = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_inmueble", x => x.fid_inmueble);
                    table.ForeignKey(
                        name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                        column: x => x.fkid_propietario,
                        principalTable: "tb_propietario",
                        principalColumn: "fid_propietario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_inmueble_tb_usuario_FkidUsuario",
                        column: x => x.FkidUsuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_inquilino",
                columns: table => new
                {
                    fid_inquilino = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fnombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fapellidos = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fcedula = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    ftelefono = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    fcelular = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    FkidUsuario = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false),
                    FfechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_inquilino", x => x.fid_inquilino);
                    table.ForeignKey(
                        name: "FK_tb_inquilino_tb_usuario_FkidUsuario",
                        column: x => x.FkidUsuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_permiso_cobros",
                columns: table => new
                {
                    fid = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_permiso_cobro", x => x.fid);
                    table.ForeignKey(
                        name: "FK_tb_permiso_cobros_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cxc",
                columns: table => new
                {
                    fid_cuenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fid_inquilino = table.Column<int>(type: "int", nullable: true),
                    fkid_inmueble = table.Column<int>(type: "int", nullable: true),
                    FfechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fdias_gracia = table.Column<int>(type: "int", nullable: false),
                    ftasa_mora = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FfechaProxCuota = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fid_periodo_pago = table.Column<int>(type: "int", nullable: false),
                    Fnota = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cxc", x => x.fid_cuenta);
                    table.ForeignKey(
                        name: "FK_tb_cxc_periodos_pago_fid_periodo_pago",
                        column: x => x.fid_periodo_pago,
                        principalTable: "periodos_pago",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_inmueble_fkid_inmueble",
                        column: x => x.fkid_inmueble,
                        principalTable: "tb_inmueble",
                        principalColumn: "fid_inmueble");
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_inquilino_fid_inquilino",
                        column: x => x.fid_inquilino,
                        principalTable: "tb_inquilino",
                        principalColumn: "fid_inquilino");
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cobros",
                columns: table => new
                {
                    fid_cobro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cxc = table.Column<int>(type: "int", nullable: false),
                    ffecha = table.Column<DateOnly>(type: "date", nullable: false),
                    fhora = table.Column<TimeOnly>(type: "time", unicode: false, maxLength: 16, nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fdescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fcargos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fconcepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FmotivoAnulacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FfechaAnulacion = table.Column<DateOnly>(type: "date", nullable: true),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fkid_origen = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobros", x => x.fid_cobro);
                    table.ForeignKey(
                        name: "FK_tb_cobros_tb_cxc_fkid_cxc",
                        column: x => x.fkid_cxc,
                        principalTable: "tb_cxc",
                        principalColumn: "fid_cuenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cxc_cuota",
                columns: table => new
                {
                    fid_cuota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fid_cxc = table.Column<int>(type: "int", nullable: false),
                    fnumero_cuota = table.Column<int>(type: "int", nullable: false),
                    fvence = table.Column<DateTime>(type: "date", nullable: false),
                    fmonto = table.Column<int>(type: "int", nullable: false),
                    fsaldo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fmora = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ffecha_ult_calculo = table.Column<DateTime>(type: "date", nullable: false),
                    fstatus = table.Column<string>(type: "char(1)", maxLength: 1, nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cxc_cuota", x => x.fid_cuota);
                    table.ForeignKey(
                        name: "FK_tb_cxc_cuota_tb_cxc_fid_cxc",
                        column: x => x.fid_cxc,
                        principalTable: "tb_cxc",
                        principalColumn: "fid_cuenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cobros_desglose",
                columns: table => new
                {
                    fidDesglose = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cobro = table.Column<int>(type: "int", nullable: false),
                    fefectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ftransferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fmonto_recibido = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ftarjeta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fnota_credito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fcheque = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fdeposito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fdebito_automatico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fno_nota_credito = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobros_desglose", x => x.fidDesglose);
                    table.ForeignKey(
                        name: "FK_tb_cobros_desglose_tb_cobros_fkid_cobro",
                        column: x => x.fkid_cobro,
                        principalTable: "tb_cobros",
                        principalColumn: "fid_cobro");
                    table.ForeignKey(
                        name: "FK_tb_cobros_desglose_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario");
                });

            migrationBuilder.CreateTable(
                name: "tb_cobros_detalle",
                columns: table => new
                {
                    fid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cobro = table.Column<int>(type: "int", nullable: false),
                    fnumeroCuota = table.Column<int>(type: "int", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fmora = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobros_detalle", x => x.fid);
                    table.ForeignKey(
                        name: "FK_tb_cobros_detalle_tb_cobros_fkid_cobro",
                        column: x => x.fkid_cobro,
                        principalTable: "tb_cobros",
                        principalColumn: "fid_cobro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "periodos_pago",
                columns: new[] { "id", "dias", "nombre" },
                values: new object[,]
                {
                    { 1, 6, "Semanal" },
                    { 2, 15, "Quincenal" },
                    { 3, 30, "Mensual" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobros_fkid_cxc",
                table: "tb_cobros",
                column: "fkid_cxc");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobros_desglose_fkid_cobro",
                table: "tb_cobros_desglose",
                column: "fkid_cobro");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobros_desglose_fkid_usuario",
                table: "tb_cobros_desglose",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobros_detalle_fkid_cobro",
                table: "tb_cobros_detalle",
                column: "fkid_cobro");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fid_inquilino",
                table: "tb_cxc",
                column: "fid_inquilino");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fid_periodo_pago",
                table: "tb_cxc",
                column: "fid_periodo_pago");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fkid_inmueble",
                table: "tb_cxc",
                column: "fkid_inmueble");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fkid_usuario",
                table: "tb_cxc",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_cuota_fid_cxc",
                table: "tb_cxc_cuota",
                column: "fid_cxc");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inmueble_fkid_propietario",
                table: "tb_inmueble",
                column: "fkid_propietario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inmueble_FkidUsuario",
                table: "tb_inmueble",
                column: "FkidUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inquilino_FkidUsuario",
                table: "tb_inquilino",
                column: "FkidUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_permiso_cobros_fkid_usuario",
                table: "tb_permiso_cobros",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuario_identity_id",
                table: "tb_usuario",
                column: "identity_id",
                unique: true,
                filter: "[identity_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "tb_auditoria");

            migrationBuilder.DropTable(
                name: "tb_cobros_desglose");

            migrationBuilder.DropTable(
                name: "tb_cobros_detalle");

            migrationBuilder.DropTable(
                name: "tb_cxc_cuota");

            migrationBuilder.DropTable(
                name: "tb_empresa");

            migrationBuilder.DropTable(
                name: "tb_permiso_cobros");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "tb_cobros");

            migrationBuilder.DropTable(
                name: "tb_cxc");

            migrationBuilder.DropTable(
                name: "periodos_pago");

            migrationBuilder.DropTable(
                name: "tb_inmueble");

            migrationBuilder.DropTable(
                name: "tb_inquilino");

            migrationBuilder.DropTable(
                name: "tb_propietario");

            migrationBuilder.DropTable(
                name: "tb_usuario");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
