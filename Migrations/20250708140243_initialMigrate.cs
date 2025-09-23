using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alquileres.Migrations
{
    /// <inheritdoc />
    public partial class initialMigrate : Migration
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
                name: "tb_empresa",
                columns: table => new
                {
                    fid_empresa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    frnc = table.Column<string>(type: "varchar(18)", nullable: true),
                    fnombre = table.Column<string>(type: "varchar(250)", nullable: true),
                    fdireccion = table.Column<string>(type: "varchar(250)", nullable: true),
                    ftelefonos = table.Column<string>(type: "varchar(14)", nullable: true),
                    festlogan = table.Column<string>(type: "varchar(250)", nullable: true),
                    fmensaje = table.Column<string>(type: "varchar(250)", nullable: true),
                    flogo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ffondo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    fcodigoqr_web = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    fcodigoqr_redes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    femail = table.Column<string>(type: "varchar(50)", nullable: true),
                    fcontraseña = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_empresa", x => x.fid_empresa);
                });

            migrationBuilder.CreateTable(
                name: "tb_moneda",
                columns: table => new
                {
                    fid_moneda = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fmoneda = table.Column<string>(type: "varchar(10)", nullable: false),
                    fsimbolo = table.Column<string>(type: "varchar(5)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_moneda", x => x.fid_moneda);
                });

            migrationBuilder.CreateTable(
                name: "tb_periodos_pago",
                columns: table => new
                {
                    fid_periodo_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fnombre = table.Column<string>(type: "varchar(20)", nullable: false),
                    fdias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_periodos_pago", x => x.fid_periodo_pago);
                });

            migrationBuilder.CreateTable(
                name: "tb_propietario",
                columns: table => new
                {
                    fid_propietario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fnombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    fapellidos = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    fcedula = table.Column<string>(type: "varchar(15)", nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ftelefono = table.Column<string>(type: "varchar(20)", nullable: false),
                    fcelular = table.Column<string>(type: "varchar(20)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false),
                    ffecha_registro = table.Column<DateTime>(type: "Date", nullable: false)
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
                    identity_id = table.Column<string>(type: "nvarchar(450)", unicode: false, maxLength: 450, nullable: true),
                    fnombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fusuario = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 50, nullable: false),
                    fnivel = table.Column<int>(type: "int", nullable: false),
                    fpassword = table.Column<string>(type: "varchar(MAX)", unicode: false, maxLength: 100, nullable: false),
                    factivado = table.Column<bool>(type: "bit", nullable: false),
                    fkid_sucursal = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false),
                    tutorial_visto = table.Column<bool>(type: "bit", maxLength: 450, nullable: false),
                    femail = table.Column<string>(type: "varchar(100)", nullable: false)
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
                name: "tb_auditoria",
                columns: table => new
                {
                    fid_auditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fkid_registro = table.Column<int>(type: "int", nullable: false),
                    ftabla = table.Column<string>(type: "varchar(50)", nullable: false),
                    ffecha = table.Column<DateTime>(type: "Date", nullable: false),
                    fhora = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    faccion = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_auditoria", x => x.fid_auditoria);
                    table.ForeignKey(
                        name: "FK_tb_auditoria_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario");
                });

            migrationBuilder.CreateTable(
                name: "tb_comprobante_fiscal",
                columns: table => new
                {
                    fid_comprobante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_empresa = table.Column<int>(type: "int", nullable: true),
                    fid_tipo_comprobante = table.Column<int>(type: "int", nullable: true),
                    fprefijo = table.Column<string>(type: "varchar(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    finicia = table.Column<int>(type: "int", nullable: true),
                    ffinaliza = table.Column<int>(type: "int", nullable: true),
                    fcontador = table.Column<int>(type: "int", nullable: true),
                    fcomprobante = table.Column<string>(type: "varchar(200)", nullable: false, computedColumnSql: "([fprefijo]+CONVERT([varchar],replicate('0',(8)-len([fcontador]+(1)))+CONVERT([varchar],[fcontador]+(1)))) PERSISTED"),
                    fvence = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "getdate()"),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    ftipo_comprobante = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    festado_sync = table.Column<string>(type: "varchar(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: false, defaultValue: "S")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_comprobante_fiscal", x => x.fid_comprobante);
                    table.ForeignKey(
                        name: "FK_tb_comprobante_fiscal_tb_empresa_fid_empresa",
                        column: x => x.fkid_empresa,
                        principalTable: "tb_empresa",
                        principalColumn: "fid_empresa");
                    table.ForeignKey(
                        name: "FK_tb_comprobante_fiscal_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_gasto_tipo",
                columns: table => new
                {
                    fid_gasto_tipo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdescripcion = table.Column<string>(type: "varchar(250)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_gasto_tipo", x => x.fid_gasto_tipo);
                    table.ForeignKey(
                        name: "FK_tb_gasto_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_inmueble",
                columns: table => new
                {
                    fid_inmueble = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_propietario = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fkid_moneda = table.Column<int>(type: "int", nullable: false),
                    fdescripcion = table.Column<string>(type: "varchar(250)", nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(200)", nullable: false),
                    fubicacion = table.Column<string>(type: "varchar(100)", nullable: false),
                    fprecio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ffecha_registro = table.Column<DateTime>(type: "Date", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_inmueble", x => x.fid_inmueble);
                    table.ForeignKey(
                        name: "FK_tb_inmueble_tb_moneda_fkid_moneda",
                        column: x => x.fkid_moneda,
                        principalTable: "tb_moneda",
                        principalColumn: "fid_moneda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_inmueble_tb_propietario_fkid_propietario",
                        column: x => x.fkid_propietario,
                        principalTable: "tb_propietario",
                        principalColumn: "fid_propietario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_inmueble_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
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
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fnombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    fapellidos = table.Column<string>(type: "varchar(100)", nullable: false),
                    fcedula = table.Column<string>(type: "varchar(15)", nullable: false),
                    fdireccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ftelefono = table.Column<string>(type: "varchar(20)", nullable: false),
                    fcelular = table.Column<string>(type: "varchar(20)", nullable: false),
                    ffecha_registro = table.Column<DateTime>(type: "Date", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_inquilino", x => x.fid_inquilino);
                    table.ForeignKey(
                        name: "FK_tb_inquilino_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_permiso_cobros",
                columns: table => new
                {
                    fid_permiso_cobro = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_permiso_cobro", x => x.fid_permiso_cobro);
                    table.ForeignKey(
                        name: "FK_tb_permiso_cobros_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_gasto",
                columns: table => new
                {
                    fid_gasto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_gasto_tipo = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdescripcion = table.Column<string>(type: "varchar(250)", nullable: false),
                    ffecha = table.Column<DateOnly>(type: "date", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_gasto", x => x.fid_gasto);
                    table.ForeignKey(
                        name: "FK_tb_gasto_tb_gasto_tipo_fkid_gasto_tipo",
                        column: x => x.fkid_gasto_tipo,
                        principalTable: "tb_gasto_tipo",
                        principalColumn: "fid_gasto_tipo");
                    table.ForeignKey(
                        name: "FK_tb_gasto_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_cxc",
                columns: table => new
                {
                    fid_cuenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_inquilino = table.Column<int>(type: "int", nullable: true),
                    fkid_inmueble = table.Column<int>(type: "int", nullable: true),
                    fid_periodo_pago = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    ffecha_inicio = table.Column<DateTime>(type: "Date", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdias_gracia = table.Column<int>(type: "int", nullable: false),
                    ftasa_mora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ffecha_prox_cuota = table.Column<DateTime>(type: "Date", nullable: false),
                    fnota = table.Column<string>(type: "varchar(250)", nullable: false),
                    fstatus = table.Column<string>(type: "varchar(1)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cxc", x => x.fid_cuenta);
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_inmueble_fkid_inmueble",
                        column: x => x.fkid_inmueble,
                        principalTable: "tb_inmueble",
                        principalColumn: "fid_inmueble");
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_inquilino_fkid_inquilino",
                        column: x => x.fkid_inquilino,
                        principalTable: "tb_inquilino",
                        principalColumn: "fid_inquilino");
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_periodos_pago_fid_periodo_pago",
                        column: x => x.fid_periodo_pago,
                        principalTable: "tb_periodos_pago",
                        principalColumn: "fid_periodo_pago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_cxc_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cobro",
                columns: table => new
                {
                    fid_cobro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cxc = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fkid_origen = table.Column<int>(type: "int", nullable: false),
                    ffecha = table.Column<DateOnly>(type: "Date", nullable: false),
                    fhora = table.Column<TimeOnly>(type: "time", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdescuento = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fcargos = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fconcepto = table.Column<string>(type: "varchar(200)", nullable: false),
                    fncf = table.Column<string>(type: "varchar(15)", nullable: false),
                    FncfVence = table.Column<DateOnly>(type: "date", nullable: true),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobro", x => x.fid_cobro);
                    table.ForeignKey(
                        name: "FK_tb_cobro_tb_cxc_fkid_cxc",
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
                    fkid_cxc = table.Column<int>(type: "int", nullable: false),
                    fnumero_cuota = table.Column<int>(type: "int", nullable: false),
                    fvence = table.Column<DateTime>(type: "Date", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fsaldo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fmora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ffecha_ult_calculo = table.Column<DateTime>(type: "Date", nullable: false),
                    fdias_atraso = table.Column<int>(type: "int", nullable: false),
                    fstatus = table.Column<string>(type: "varchar(1)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cxc_cuota", x => x.fid_cuota);
                    table.ForeignKey(
                        name: "FK_tb_cxc_cuota_tb_cxc_fkid_cxc",
                        column: x => x.fkid_cxc,
                        principalTable: "tb_cxc",
                        principalColumn: "fid_cuenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_cxc_nulo",
                columns: table => new
                {
                    fid_cxc_nulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cuenta = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fhora = table.Column<TimeOnly>(type: "time", nullable: false),
                    fmotivo_anulacion = table.Column<string>(type: "varchar(250)", nullable: false),
                    ffecha_anulacion = table.Column<DateOnly>(type: "Date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cxc_nulo", x => x.fid_cxc_nulo);
                    table.ForeignKey(
                        name: "FK_tb_cxc_nulo_tb_cxc_fkid_cuenta",
                        column: x => x.fkid_cuenta,
                        principalTable: "tb_cxc",
                        principalColumn: "fid_cuenta");
                    table.ForeignKey(
                        name: "FK_tb_cxc_nulo_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario");
                });

            migrationBuilder.CreateTable(
                name: "tb_cobro_nulo",
                columns: table => new
                {
                    fid_cobro_nulo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cobro = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    fhora = table.Column<TimeOnly>(type: "time", nullable: false),
                    fmotivo_anulacion = table.Column<string>(type: "varchar(250)", nullable: false),
                    ffecha_anulacion = table.Column<DateOnly>(type: "Date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobro_nulo", x => x.fid_cobro_nulo);
                    table.ForeignKey(
                        name: "FK_tb_cobro_nulo_tb_cobro_fkid_cobro",
                        column: x => x.fkid_cobro,
                        principalTable: "tb_cobro",
                        principalColumn: "fid_cobro");
                    table.ForeignKey(
                        name: "FK_tb_cobro_nulo_tb_usuario_fkid_usuario",
                        column: x => x.fkid_usuario,
                        principalTable: "tb_usuario",
                        principalColumn: "fid_usuario");
                });

            migrationBuilder.CreateTable(
                name: "tb_cobros_desglose",
                columns: table => new
                {
                    fid_desglose = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cobro = table.Column<int>(type: "int", nullable: false),
                    fefectivo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ftransferencia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fmonto_recibido = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ftarjeta = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fnota_credito = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fcheque = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdeposito = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fdebito_automatico = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fno_nota_credito = table.Column<int>(type: "int", nullable: false),
                    fkid_usuario = table.Column<int>(type: "int", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobros_desglose", x => x.fid_desglose);
                    table.ForeignKey(
                        name: "FK_tb_cobros_desglose_tb_cobro_fkid_cobro",
                        column: x => x.fkid_cobro,
                        principalTable: "tb_cobro",
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
                    fid_cobro_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkid_cobro = table.Column<int>(type: "int", nullable: false),
                    fnumero_cuota = table.Column<int>(type: "int", nullable: false),
                    fmonto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fmora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    factivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_cobros_detalle", x => x.fid_cobro_detalle);
                    table.ForeignKey(
                        name: "FK_tb_cobros_detalle_tb_cobro_fkid_cobro",
                        column: x => x.fkid_cobro,
                        principalTable: "tb_cobro",
                        principalColumn: "fid_cobro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "tb_periodos_pago",
                columns: new[] { "fid_periodo_pago", "fdias", "fnombre" },
                values: new object[] { 1, 30, "Mensual" });

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
                name: "IX_tb_auditoria_fkid_usuario",
                table: "tb_auditoria",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobro_fkid_cxc",
                table: "tb_cobro",
                column: "fkid_cxc");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobro_nulo_fkid_cobro",
                table: "tb_cobro_nulo",
                column: "fkid_cobro");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cobro_nulo_fkid_usuario",
                table: "tb_cobro_nulo",
                column: "fkid_usuario");

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
                name: "IX_tb_comprobante_fiscal_fkid_empresa",
                table: "tb_comprobante_fiscal",
                column: "fkid_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_tb_comprobante_fiscal_fkid_usuario",
                table: "tb_comprobante_fiscal",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fid_periodo_pago",
                table: "tb_cxc",
                column: "fid_periodo_pago");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fkid_inmueble",
                table: "tb_cxc",
                column: "fkid_inmueble");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fkid_inquilino",
                table: "tb_cxc",
                column: "fkid_inquilino");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_fkid_usuario",
                table: "tb_cxc",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_cuota_fkid_cxc",
                table: "tb_cxc_cuota",
                column: "fkid_cxc");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_nulo_fkid_cuenta",
                table: "tb_cxc_nulo",
                column: "fkid_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_tb_cxc_nulo_fkid_usuario",
                table: "tb_cxc_nulo",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_gasto_fkid_gasto_tipo",
                table: "tb_gasto",
                column: "fkid_gasto_tipo");

            migrationBuilder.CreateIndex(
                name: "IX_tb_gasto_fkid_usuario",
                table: "tb_gasto",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_gasto_tipo_fkid_usuario",
                table: "tb_gasto_tipo",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inmueble_fkid_moneda",
                table: "tb_inmueble",
                column: "fkid_moneda");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inmueble_fkid_propietario",
                table: "tb_inmueble",
                column: "fkid_propietario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inmueble_fkid_usuario",
                table: "tb_inmueble",
                column: "fkid_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_inquilino_fkid_usuario",
                table: "tb_inquilino",
                column: "fkid_usuario");

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
                name: "tb_cobro_nulo");

            migrationBuilder.DropTable(
                name: "tb_cobros_desglose");

            migrationBuilder.DropTable(
                name: "tb_cobros_detalle");

            migrationBuilder.DropTable(
                name: "tb_comprobante_fiscal");

            migrationBuilder.DropTable(
                name: "tb_cxc_cuota");

            migrationBuilder.DropTable(
                name: "tb_cxc_nulo");

            migrationBuilder.DropTable(
                name: "tb_gasto");

            migrationBuilder.DropTable(
                name: "tb_permiso_cobros");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "tb_cobro");

            migrationBuilder.DropTable(
                name: "tb_empresa");

            migrationBuilder.DropTable(
                name: "tb_gasto_tipo");

            migrationBuilder.DropTable(
                name: "tb_cxc");

            migrationBuilder.DropTable(
                name: "tb_inmueble");

            migrationBuilder.DropTable(
                name: "tb_inquilino");

            migrationBuilder.DropTable(
                name: "tb_periodos_pago");

            migrationBuilder.DropTable(
                name: "tb_moneda");

            migrationBuilder.DropTable(
                name: "tb_propietario");

            migrationBuilder.DropTable(
                name: "tb_usuario");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
