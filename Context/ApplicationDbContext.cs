using System.Security.Claims;
using Alquileres.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Alquileres.Context
{
    public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context; // Asegúrate de que esto esté definido
        private readonly ILogger<ApplicationDbContext> _logger; // Asegúrate de que esto esté definido

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger; // Inicializa el logger
            _context = this; // Inicializa el contexto
        }


        public virtual DbSet<TbPeriodoPago> PeriodosPagos { get; set; }
        public virtual DbSet<TbAuditorium> TbAuditoria { get; set; }
        public virtual DbSet<TbCobro> TbCobros { get; set; }
        public virtual DbSet<TbCobrosDetalle> TbCobrosDetalles { get; set; }
        public virtual DbSet<TbCobrosDesglose> TbCobrosDesgloses { get; set; }
        public virtual DbSet<TbCxc> TbCxcs { get; set; }
        public virtual DbSet<TbCxcCuotum> TbCxcCuota { get; set; }
        public virtual DbSet<TbInmueble> TbInmuebles { get; set; }
        public virtual DbSet<TbInquilino> TbInquilinos { get; set; }
        public virtual DbSet<TbPermisoCobro> TbPermisoCobros { get; set; }
        public virtual DbSet<TbPropietario> TbPropietarios { get; set; }
        public virtual DbSet<TbUsuario> TbUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TbPeriodoPago>().HasData(
                new TbPeriodoPago { Id = 1, Nombre = "Semanal", Dias = 6 },
                new TbPeriodoPago { Id = 2, Nombre = "Quincenal", Dias = 15 },
                new TbPeriodoPago { Id = 3, Nombre = "Mensual", Dias = 30 }
            );


            modelBuilder.Entity<TbPeriodoPago>(entity =>
            {
                entity.ToTable("periodos_pago");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Dias).HasColumnName("dias");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<TbAuditorium>(entity =>
            {
                entity.HasKey(e => e.Fid);

                entity.ToTable("tb_auditoria");

                entity.Property(e => e.Fid).HasColumnName("fid");
                entity.Property(e => e.Faccion)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("faccion");
                entity.Property(e => e.Ffecha).HasColumnName("ffecha");
                entity.Property(e => e.Fhora)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasColumnName("fhora");
                entity.Property(e => e.FkidRegistro).HasColumnName("fkid_registro");
                entity.Property(e => e.FkidUsuario).HasColumnName("fkid_usuario");
                entity.Property(e => e.Ftabla)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ftabla");
            });

            modelBuilder.Entity<TbCobro>(entity =>
            {
                entity.HasKey(e => e.FidCobro);

                entity.ToTable("tb_cobros");

                entity.Property(e => e.FidCobro).HasColumnName("fid_cobro");
                entity.Property(e => e.Factivo).HasColumnName("factivo");
                entity.Property(e => e.Fcargos)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fcargos");
                entity.Property(e => e.Fconcepto)
                    .HasMaxLength(255)
                    .IsFixedLength()
                    .HasColumnName("fconcepto");
                entity.Property(e => e.Fdescuento)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fdescuento");
                entity.Property(e => e.Ffecha)
                    .HasColumnName("ffecha");
                entity.Property(e => e.Fhora)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .HasColumnName("fhora");
                entity.Property(e => e.FkidCxc).HasColumnName("fkid_cxc");
                entity.Property(e => e.FkidOrigen).HasColumnName("fkid_origen");
                entity.Property(e => e.FkidUsuario).HasColumnName("fkid_usuario");
                entity.Property(e => e.Fmonto)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmonto");

                entity.HasOne<TbCxc>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidCxc);
            });

            modelBuilder.Entity<TbCobrosDetalle>(entity =>
            {
                entity.HasKey(e => e.Fid);

                entity.ToTable("tb_cobros_detalle");

                entity.Property(e => e.Fid)
                    .HasColumnName("fid");
                entity.Property(e => e.Factivo).HasColumnName("factivo");
                entity.Property(e => e.FkidCobro).HasColumnName("fkid_cobro");
                entity.Property(e => e.Fmonto)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmonto");
                entity.Property(e => e.Fmora)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmora");
                entity.Property(e => e.FnumeroCuota).HasColumnName("fnumeroCuota");

                entity.HasOne<TbCobro>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidCobro);
            });

            modelBuilder.Entity<TbCobrosDesglose>(entity =>
            {
                entity.HasKey(e => e.FidDesglose);

                entity.ToTable("tb_cobros_desglose");

                entity.Property(e => e.FidDesglose)
                    .HasColumnName("fidDesglose");
                entity.Property(e => e.FkidCobro)
                    .HasColumnName("fkid_cobro");
                entity.Property(e => e.FkidUsuario)
                    .HasColumnName("fkid_usuario");

                entity.Property(e => e.Fefectivo)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fefectivo");
                entity.Property(e => e.Ftransferencia)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("ftransferencia");
                entity.Property(e => e.FmontoRecibido)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmonto_recibido");
                entity.Property(e => e.Ftarjeta)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("ftarjeta");
                entity.Property(e => e.FnotaCredito)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fnota_credito");
                entity.Property(e => e.Fcheque)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fcheque");
                entity.Property(e => e.Fdeposito)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fdeposito");
                entity.Property(e => e.FdebitoAutomatico)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fdebito_automatico");
                entity.Property(e => e.FnoNotaCredito)
                    .HasColumnType("int")
                    .HasColumnName("fno_nota_credito");
                entity.Property(e => e.Factivo)
                    .HasColumnName("factivo");

                // Relaciones con la tabla cobro y la tabla usuario
                entity.HasOne<TbCobro>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidCobro)
                    .OnDelete(DeleteBehavior.NoAction); // Cambiar la cascada por "NoAction"

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario)
                    .OnDelete(DeleteBehavior.NoAction); // Cambiar la cascada por "NoAction"
            });


            modelBuilder.Entity<TbCxc>(entity =>
            {
                entity.HasKey(e => e.FidCuenta);

                entity.ToTable("tb_cxc");

                entity.HasIndex(e => e.FidInquilino, "IX_tb_cxc_fid_inquilino");
                entity.HasIndex(e => e.FidPeriodoPago, "IX_tb_cxc_fid_periodo_pago");
                entity.HasIndex(e => e.FkidInmueble, "IX_tb_cxc_fkid_inmueble");
                entity.HasIndex(e => e.FkidUsuario, "IX_tb_cxc_fkid_usuario");

                entity.Property(e => e.FidCuenta).HasColumnName("fid_cuenta");
                entity.Property(e => e.FdiasGracia).HasColumnName("fdias_gracia");
                entity.Property(e => e.FidInquilino).HasColumnName("fid_inquilino");
                entity.Property(e => e.FidPeriodoPago).HasColumnName("fid_periodo_pago");
                entity.Property(e => e.FkidInmueble).HasColumnName("fkid_inmueble");
                entity.Property(e => e.FkidUsuario).HasColumnName("fkid_usuario");
                entity.Property(e => e.Fmonto)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmonto");
                entity.Property(e => e.FtasaMora)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("ftasa_mora");

                entity.HasOne<TbInquilino>()
                    .WithMany()
                    .HasForeignKey(e => e.FidInquilino);

                entity.HasOne<TbPeriodoPago>()
                    .WithMany()
                    .HasForeignKey(e => e.FidPeriodoPago);

                entity.HasOne<TbInmueble>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidInmueble);

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);

            });

            modelBuilder.Entity<TbCxcCuotum>(entity =>
            {
                entity.HasKey(e => e.FidCuota);

                entity.ToTable("tb_cxc_cuota");

                entity.Property(e => e.FidCuota)
                    .HasColumnName("fid_cuota");

                entity.Property(e => e.Factivo)
                    .HasColumnName("factivo");

                entity.Property(e => e.FfechaUltCalculo)
                    .HasColumnType("date")
                    .HasColumnName("ffecha_ult_calculo");

                entity.Property(e => e.FidCxc)
                    .HasColumnName("fid_cxc");

                entity.Property(e => e.Fmonto)
                    .HasColumnName("fmonto");

                entity.Property(e => e.Fmora)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fmora");

                entity.Property(e => e.FNumeroCuota)
                    .HasColumnName("fnumero_cuota");

                entity.Property(e => e.Fsaldo)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fsaldo");

                entity.Property(e => e.Fvence)
                    .HasColumnType("date")
                    .HasColumnName("fvence");

                entity.Property(e => e.Fstatus)
                    .HasColumnType("char")
                    .HasColumnName("fstatus")
                    .HasMaxLength(1);

                // Configuración de la relación con TbCxc
                entity.HasOne<TbCxc>()
                    .WithMany() // Aquí puedes especificar el nombre de la colección si es necesario
                    .HasForeignKey(e => e.FidCxc)
                    .OnDelete(DeleteBehavior.Cascade); // Ajusta el comportamiento de eliminación según sea necesario
            });

            modelBuilder.Entity<TbInmueble>(entity =>
            {
                entity.HasKey(e => e.FidInmueble);

                entity.ToTable("tb_inmueble");

                entity.HasIndex(e => e.FkidPropietario, "IX_tb_inmueble_fkid_propietario");

                entity.Property(e => e.FidInmueble).HasColumnName("fid_inmueble");
                entity.Property(e => e.Factivo)
                    .HasColumnName("factivo")
                    .HasDefaultValue(true);

                entity.Property(e => e.Fdescripcion)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("fdescripcion");

                entity.Property(e => e.Fdireccion)
                    .IsRequired()
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("fdireccion");

                entity.Property(e => e.FkidPropietario)
                    .HasColumnName("fkid_propietario"); // Corregí el nombre

                entity.Property(e => e.Fprecio)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("fprecio");

                entity.Property(e => e.Fubicacion)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fubicacion");

                entity.Property(e => e.FfechaRegistro)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasColumnName("ffechaRegistro");

                entity.HasOne<TbPropietario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidPropietario)
                    .OnDelete(DeleteBehavior.Restrict); // Añade comportamiento de borrado
            });

            modelBuilder.Entity<TbInquilino>(entity =>
            {
                entity.HasKey(e => e.FidInquilino);

                entity.ToTable("tb_inquilino");

                entity.Property(e => e.FidInquilino).HasColumnName("fid_inquilino");
                entity.Property(e => e.Factivo).HasColumnName("factivo");
                entity.Property(e => e.Fapellidos)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fapellidos");
                entity.Property(e => e.Fcedula)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("fcedula");
                entity.Property(e => e.Fcelular)
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .HasColumnName("fcelular");
                entity.Property(e => e.Fdireccion)
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("fdireccion");
                entity.Property(e => e.Fnombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fnombre");
                entity.Property(e => e.Ftelefono)
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .HasColumnName("ftelefono");
                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);
            });

            modelBuilder.Entity<TbPermisoCobro>(entity =>
            {
                entity.HasKey(e => e.Fid).HasName("PK_tb_permiso_cobro");

                entity.ToTable("tb_permiso_cobros");

                entity.Property(e => e.Fid)
                    .ValueGeneratedNever()
                    .HasColumnName("fid");
                entity.Property(e => e.FkidUsuario).HasColumnName("fkid_usuario");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);
            });

            modelBuilder.Entity<TbPropietario>(entity =>
            {
                entity.HasKey(e => e.FidPropietario);

                entity.ToTable("tb_propietario");

                entity.Property(e => e.FidPropietario).HasColumnName("fid_propietario");
                entity.Property(e => e.Factivo).HasColumnName("factivo");
                entity.Property(e => e.Fapellidos)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fapellidos");
                entity.Property(e => e.Fcedula)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("fcedula");
                entity.Property(e => e.Fcelular)
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .HasColumnName("fcelular");
                entity.Property(e => e.Fdireccion)
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("fdireccion");
                entity.Property(e => e.Fnombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fnombre");
                entity.Property(e => e.Ftelefono)
                    .HasMaxLength(14)
                    .IsUnicode(false)
                    .HasColumnName("ftelefono");
            });

            modelBuilder.Entity<TbUsuario>(entity =>
            {
                entity.HasKey(e => e.FidUsuario);

                entity.ToTable("tb_usuario");

                // Configuración de la relación con AspNetUsers
                entity.HasIndex(e => e.IdentityId)
                    .IsUnique()
                    .HasDatabaseName("IX_tb_usuario_identity_id");

                entity.Property(e => e.IdentityId)
                    .HasMaxLength(450) // Asegúrate de que la longitud coincida con AspNetUsers.Id
                    .IsUnicode(false)   // Asegúrate de que sea no Unicode
                    .HasColumnName("identity_id");

                // Configuración de la relación usando Fluent API
                entity.HasOne<IdentityUser>()  // Relación con AspNetUsers
                    .WithOne()                   // Relación uno a uno
                    .HasForeignKey<TbUsuario>(u => u.IdentityId)  // FK a AspNetUsers.Id
                    .IsRequired(false)           // Si permite valores nulos
                    .OnDelete(DeleteBehavior.Restrict);

                // Resto de propiedades
                entity.Property(e => e.FidUsuario)
                    .HasColumnName("fid_usuario");
                entity.Property(e => e.Factivado).HasColumnName("factivado");
                entity.Property(e => e.Factivo).HasColumnName("factivo");
                entity.Property(e => e.FestadoSync)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasDefaultValue("A")
                    .IsFixedLength()
                    .HasColumnName("festado_sync");
                entity.Property(e => e.FkidSucursal).HasColumnName("fkid_sucursal");
                entity.Property(e => e.Fnivel).HasColumnName("fnivel");
                entity.Property(e => e.Fnombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fnombre");
                entity.Property(e => e.Fpassword)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("fpassword");
                entity.Property(e => e.Fusuario)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("fusuario");
                entity.Property(u => u.IdentityId)
                    .HasMaxLength(450)
                    .HasColumnType("nvarchar(450)")
                    .HasColumnName("identity_id"); ;
            });

            OnModelCreatingPartial(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var auditorias = new List<TbAuditorium>();
            var usuarioIdActual = await ObtenerUsuarioIdActual(); // Asegúrate de usar await aquí

            foreach (var entrada in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            {
                var auditoria = new TbAuditorium
                {
                    Ftabla = entrada.Entity.GetType().Name,
                    Faccion = entrada.State.ToString(),
                    Ffecha = DateTime.UtcNow.Date, // Usar UTC para consistencia
                    Fhora = DateTime.UtcNow.ToString("HH:mm:ss"),
                    FkidUsuario = usuarioIdActual ?? 0, // Si no hay usuario, usar 0
                    FkidRegistro = ObtenerIdRegistro(entrada)
                };
                auditorias.Add(auditoria);
            }

            if (auditorias.Any())
            {
                await using var transaccion = await Database.BeginTransactionAsync(cancellationToken);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                await TbAuditoria.AddRangeAsync(auditorias, cancellationToken);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                await transaccion.CommitAsync(cancellationToken);
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private async Task<int?> ObtenerUsuarioIdActual()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // Verificar si ya tenemos el ID almacenado en el contexto para esta solicitud
            if (httpContext.Items.TryGetValue("CurrentUserId", out var cachedId))
            {
                return (int?)cachedId;
            }

            // Obtener Identity ID del usuario autenticado
            var identityId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityId)) return null;

            try
            {
                // Buscar el usuario en TbUsuarios usando el IdentityId
                var usuarioId = await _context.Set<TbUsuario>()
                    .Where(u => u.IdentityId == identityId)
                    .Select(u => (int?)u.FidUsuario)
                    .FirstOrDefaultAsync();

                // Cachear el resultado para esta solicitud
                httpContext.Items["CurrentUserId"] = usuarioId;

                return usuarioId;
            }
            catch (Exception ex)
            {
                // Loggear error sin interrumpir el flujo
                _logger.LogError(ex, "Error obteniendo ID de usuario");
                return null;
            }
        }

        private int ObtenerIdRegistro(EntityEntry entrada)
        {
            var clave = entrada.Properties.First(p => p.Metadata.IsPrimaryKey()).CurrentValue;

            return clave switch
            {
                int id => id,
                string s when int.TryParse(s, out var idParsed) => idParsed,
                _ => 0 // o lanza una excepción si prefieres no permitirlo
            };
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}