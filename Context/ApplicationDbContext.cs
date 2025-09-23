using System.Security.Claims;
using Alquileres.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
        public virtual DbSet<Empresa> Empresas { get; set; }

        public virtual DbSet<TbPeriodoPago> PeriodosPagos { get; set; }
        public virtual DbSet<TbAuditorium> TbAuditoria { get; set; }
        public virtual DbSet<TbCobro> TbCobros { get; set; }
        public virtual DbSet<TbCobrosDetalle> TbCobrosDetalles { get; set; }
        public virtual DbSet<TbCobrosDesglose> TbCobrosDesgloses { get; set; }
        public virtual DbSet<TbCobroNulo> TbCobrosNulos { get; set; }
        public virtual DbSet<TbCxc> TbCxcs { get; set; }
        public virtual DbSet<TbCxcNulo> TbCxcNulos { get; set; }
        public virtual DbSet<TbCxcCuotum> TbCxcCuota { get; set; }
        public virtual DbSet<TbInmueble> TbInmuebles { get; set; }
        public virtual DbSet<TbInquilino> TbInquilinos { get; set; }
        public virtual DbSet<TbPermisoCobro> TbPermisoCobros { get; set; }
        public virtual DbSet<TbPropietario> TbPropietarios { get; set; }
        public virtual DbSet<TbUsuario> TbUsuarios { get; set; }
        public virtual DbSet<TbComprobanteFiscal> TbComprobantesFiscales { get; set; }
        public virtual DbSet<TbGasto> TbGastos { get; set; }
        public virtual DbSet<TbGastoTipo> TbGastoTipos { get; set; }
        public virtual DbSet<TbMoneda> TbMonedas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserTutorialStatus>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_UserTutorialStatus");
            });

            modelBuilder.Entity<TbPeriodoPago>().HasData(
                new TbPeriodoPago { FidPeriodoPago = 1, Fnombre = "Mensual", Fdias = 30 }
            );


            // Relaciones entre las tablas: 

            // Dentro del método OnModelCreating, antes de OnModelCreatingPartial
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("tb_empresa");

                entity.HasKey(e => e.IdEmpresa)
                    .HasName("PK_tb_empresa");

                entity.Property(e => e.IdEmpresa)
                    .HasColumnName("fid_empresa")
                    .ValueGeneratedOnAdd(); // O ValueGeneratedOnAdd() si es autoincremental

            });


            modelBuilder.Entity<TbPeriodoPago>(entity =>
            {
                entity.ToTable("tb_periodos_pago");

                entity.Property(e => e.FidPeriodoPago).HasColumnName("fid_periodo_pago");

            });

            modelBuilder.Entity<TbMoneda>(entity =>
            {
                entity.ToTable("tb_moneda");

                entity.Property(e => e.FidMoneda).HasColumnName("fid_moneda");

            });

            modelBuilder.Entity<TbAuditorium>(entity =>
            {
                entity.HasKey(e => e.FidAuditoria);

                entity.ToTable("tb_auditoria");

                entity.Property(e => e.FidAuditoria).HasColumnName("fid_auditoria");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario)
                    .OnDelete(DeleteBehavior.NoAction); // Cambiar la cascada por "NoAction"

            });

            modelBuilder.Entity<TbCobro>(entity =>
            {
                entity.HasKey(e => e.FidCobro);

                entity.ToTable("tb_cobro");

                entity.Property(e => e.FidCobro).HasColumnName("fid_cobro");

                entity.HasOne<TbCxc>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidCxc);
            });

            modelBuilder.Entity<TbCobrosDetalle>(entity =>
            {
                entity.HasKey(e => e.FidCobroDetalle);

                entity.ToTable("tb_cobros_detalle");

                entity.Property(e => e.FidCobroDetalle)
                    .HasColumnName("fid_cobro_detalle");

                entity.HasOne<TbCobro>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidCobro);
            });

            modelBuilder.Entity<TbCobrosDesglose>(entity =>
            {
                entity.HasKey(e => e.FidDesglose);

                entity.ToTable("tb_cobros_desglose");

                entity.Property(e => e.FidDesglose)
                    .HasColumnName("fid_desglose");

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

            modelBuilder.Entity<TbCobroNulo>(entity =>
            {
                entity.HasKey(e => e.FidCobroNulo);

                entity.ToTable("tb_cobro_nulo");

                entity.Property(e => e.FidCobroNulo)
                    .HasColumnName("fid_cobro_nulo");

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

            modelBuilder.Entity<TbCxcNulo>(entity =>
           {
               entity.HasKey(e => e.FidCuentaNulo);

               entity.ToTable("tb_cxc_nulo");

               entity.Property(e => e.FidCuentaNulo)
                   .HasColumnName("fid_cxc_nulo");

               entity.HasOne<TbCxc>()
                   .WithMany()
                   .HasForeignKey(e => e.FkidCuenta)
                   .OnDelete(DeleteBehavior.NoAction); // Cambiar la cascada por "NoAction"

               entity.HasOne<TbUsuario>()
                   .WithMany()
                   .HasForeignKey(e => e.FkidUsuario)
                   .OnDelete(DeleteBehavior.NoAction); // Cambiar la cascada por "NoAction"
           });

            modelBuilder.Entity<TbGastoTipo>(entity =>
            {
                entity.HasKey(e => e.FidGastoTipo);

                entity.ToTable("tb_gasto_tipo");

                entity.Property(e => e.FidGastoTipo)
                    .HasColumnName("fid_gasto_tipo");

            });

            modelBuilder.Entity<TbGasto>(entity =>
            {
                entity.HasKey(e => e.FidGasto);

                entity.ToTable("tb_gasto");

                entity.Property(e => e.FidGasto)
                    .HasColumnName("fid_gasto");

                entity.HasOne<TbGastoTipo>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidGastoTipo)
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

                entity.Property(e => e.FidCuenta).HasColumnName("fid_cuenta");


                entity.HasOne<TbInquilino>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidInquilino);

                entity.HasOne<TbPeriodoPago>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidPeriodoPago)
                    .OnDelete(DeleteBehavior.Restrict);

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

                // Configuración de la relación con TbCxc
                entity.HasOne<TbCxc>()
                    .WithMany() // Aquí puedes especificar el nombre de la colección si es necesario
                    .HasForeignKey(e => e.FkidCxc)
                    .OnDelete(DeleteBehavior.Cascade); // Ajusta el comportamiento de eliminación según sea necesario
            });

            modelBuilder.Entity<TbInmueble>(entity =>
            {
                entity.HasKey(e => e.FidInmueble);

                entity.ToTable("tb_inmueble");

                entity.HasIndex(e => e.FkidPropietario, "IX_tb_inmueble_fkid_propietario");

                entity.Property(e => e.FidInmueble).HasColumnName("fid_inmueble");

                entity.HasOne<TbPropietario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidPropietario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);

                entity.HasOne<TbMoneda>()
                .WithMany()
                .HasForeignKey(e => e.FkidMoneda);
            });

            modelBuilder.Entity<TbInquilino>(entity =>
            {
                entity.HasKey(e => e.FidInquilino);

                entity.ToTable("tb_inquilino");

                entity.Property(e => e.FidInquilino).HasColumnName("fid_inquilino");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);
            });

            modelBuilder.Entity<TbPermisoCobro>(entity =>
            {
                entity.HasKey(e => e.FidPermisoCobro).HasName("PK_tb_permiso_cobro");

                entity.ToTable("tb_permiso_cobros");

                entity.Property(e => e.FidPermisoCobro)
                    .ValueGeneratedNever()
                    .HasColumnName("fid_permiso_cobro");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario);
            });

            modelBuilder.Entity<TbPropietario>(entity =>
            {
                entity.HasKey(e => e.FidPropietario);

                entity.ToTable("tb_propietario");

                entity.Property(e => e.FidPropietario)
                    .HasColumnName("fid_propietario");

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
                    .HasColumnName("identity_id");
                entity.Property(u => u.FTutorialVisto)
                    .HasMaxLength(450)
                    .HasColumnType("bit")
                    .HasColumnName("tutorial_visto");
            });

            modelBuilder.Entity<TbComprobanteFiscal>(entity =>
            {
                entity.HasKey(e => e.FidComprobante)
                    .HasName("PK_tb_comprobante_fiscal");

                entity.ToTable("tb_comprobante_fiscal");

                entity.Property(e => e.FidComprobante)
                    .HasColumnName("fid_comprobante");

                entity.Property(e => e.FkidEmpresa)
                    .HasColumnName("fkid_empresa");

                entity.Property(e => e.FidTipoComprobante)
                    .HasColumnName("fid_tipo_comprobante");

                entity.Property(e => e.Fprefijo)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("fprefijo");

                entity.Property(e => e.Finicia)
                    .HasColumnName("finicia");

                entity.Property(e => e.Ffinaliza)
                    .HasColumnName("ffinaliza");

                entity.Property(e => e.Fcontador)
                    .HasColumnName("fcontador");

                entity.Property(e => e.Fcomprobante)
                    .HasComputedColumnSql("([fprefijo]+CONVERT([varchar],replicate('0',(8)-len([fcontador]+(1)))+CONVERT([varchar],[fcontador]+(1)))) PERSISTED")
                    .HasColumnName("fcomprobante");

                entity.Property(e => e.Fvence)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getdate()")
                    .HasColumnName("fvence");

                entity.Property(e => e.FkidUsuario)
                    .HasColumnName("fkid_usuario");

                entity.Property(e => e.FtipoComprobante)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ftipo_comprobante");

                entity.Property(e => e.FestadoSync)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasDefaultValue("S")
                    .HasColumnName("festado_sync");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario)
                    .HasConstraintName("FK_tb_comprobante_fiscal_tb_usuario_fkid_usuario")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Empresa>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidEmpresa)
                    .HasConstraintName("FK_tb_comprobante_fiscal_tb_empresa_fid_empresa")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TbGasto>(entity =>
            {
                entity.HasKey(e => e.FidGasto)
                    .HasName("PK_tb_gasto");

                entity.ToTable("tb_gasto");

                entity.Property(e => e.FidGasto)
                    .HasColumnName("fid_gasto");

                entity.HasOne<TbUsuario>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<TbGastoTipo>()
                    .WithMany()
                    .HasForeignKey(e => e.FkidGastoTipo)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<TbGastoTipo>(entity =>
           {
               entity.HasKey(e => e.FidGastoTipo)
                   .HasName("PK_tb_gasto_tipo");

               entity.ToTable("tb_gasto_tipo");

               entity.Property(e => e.FidGastoTipo)
                   .HasColumnName("fid_gasto_tipo");

               entity.HasOne<TbUsuario>()
                   .WithMany()
                   .HasForeignKey(e => e.FkidUsuario)
                   .HasConstraintName("FK_tb_gasto_fkid_usuario")
                   .OnDelete(DeleteBehavior.Restrict);

           });

            OnModelCreatingPartial(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            // Saltar auditoría durante la inicialización de datos
            if (IsSeeding)
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            // Saltar auditoría para tablas de Identity framework y TbUsuario
            var entriesToSkip = ChangeTracker.Entries()
                .Where(e => e.Entity is IdentityUser ||
                            e.Entity is IdentityRole ||
                            e.Entity is IdentityUserClaim<string> ||
                            e.Entity is IdentityUserLogin<string> ||
                            e.Entity is IdentityUserToken<string> ||
                            e.Entity is IdentityRoleClaim<string> ||
                            e.Entity is TbUsuario);

            if (entriesToSkip.Any())
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            var auditorias = new List<TbAuditorium>();
            var usuarioIdActual = await ObtenerUsuarioActual();

            var entriesToAudit = ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged &&
                            !entriesToSkip.Any(s => s.Entity == e.Entity));

            foreach (var entrada in entriesToAudit)
            {
                var auditoria = new TbAuditorium
                {
                    Ftabla = entrada.Entity.GetType().Name,
                    Faccion = TraducirEstadoEntidad(entrada.State),
                    Ffecha = DateTime.UtcNow.Date,
                    Fhora = DateTime.UtcNow.ToString("HH:mm:ss"),
                    FkidUsuario = usuarioIdActual.usuarioId ?? 0,
                    FkidRegistro = ObtenerIdRegistro(entrada)
                };
                auditorias.Add(auditoria);
            }

            if (auditorias.Any())
            {
                await using var transaccion = await Database.BeginTransactionAsync(cancellationToken);
                var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                await TbAuditoria.AddRangeAsync(auditorias, cancellationToken);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                await transaccion.CommitAsync(cancellationToken);
                return result;
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private string TraducirEstadoEntidad(EntityState estado)
        {
            switch (estado)
            {
                case EntityState.Added:
                    return "Creación";
                case EntityState.Modified:
                    return "Modificación";
                case EntityState.Deleted:
                    return "Eliminación";
                case EntityState.Detached:
                    return "Desconectado";
                case EntityState.Unchanged:
                    return "Sin cambios";
                default:
                    return estado.ToString();
            }
        }

        // Add this property to track seeding state
        public bool IsSeeding { get; set; }

        private async Task<(int? usuarioId, string usuarioNombre)> ObtenerUsuarioActual()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return (null, null);

            // Verificar si ya tenemos los datos almacenados en el contexto para esta solicitud
            if (httpContext.Items.TryGetValue("CurrentUserData", out var cachedData))
            {
                return ((int? usuarioId, string usuarioNombre))cachedData;
            }

            // Obtener Identity ID del usuario autenticado
            var identityId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityId)) return (null, null);

            try
            {
                // Buscar el usuario en TbUsuarios usando el IdentityId
                var usuario = await _context.Set<TbUsuario>()
                    .Where(u => u.IdentityId == identityId)
                    .Select(u => new { u.FidUsuario, u.Fusuario })
                    .FirstOrDefaultAsync();

                if (usuario == null) return (null, null);

                // Cachear el resultado para esta solicitud
                var result = ((int?)usuario.FidUsuario, usuario.Fusuario); // Explicitly cast to int?
                httpContext.Items["CurrentUserData"] = result;

                return result;
            }
            catch (Exception ex)
            {
                // Loggear error sin interrumpir el flujo
                _logger.LogError(ex, "Error obteniendo datos de usuario");
                return (null, null);
            }
        }

        private int ObtenerIdRegistro(EntityEntry entrada)
        {
            try
            {
                // 1. Primero intentamos obtener el ID real (para entidades existentes)
                if (entrada.State != EntityState.Added)
                {
                    var propClave = entrada.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    if (propClave?.CurrentValue != null)
                    {
                        // Handle different ID types
                        return propClave.CurrentValue switch
                        {
                            int intValue => intValue,
                            long longValue => (int)longValue,
                            string strValue when int.TryParse(strValue, out var parsedInt) => parsedInt,
                            Guid guidValue => Math.Abs(guidValue.GetHashCode()), // Convert GUID to a pseudo-int
                            _ => 0
                        };
                    }
                }

                // 2. Para entidades nuevas, generamos un ID secuencial temporal
                if (entrada.State == EntityState.Added)
                {
                    // Null check for HttpContext
                    if (_httpContextAccessor.HttpContext == null)
                    {
                        return -1;
                    }

                    // Usamos un contador estático para IDs temporales (por solicitud)
                    if (!_httpContextAccessor.HttpContext.Items.ContainsKey("TempIdCounter"))
                    {
                        _httpContextAccessor.HttpContext.Items["TempIdCounter"] = -1;
                    }

                    var counter = (int)_httpContextAccessor.HttpContext.Items["TempIdCounter"]!;
                    counter--;
                    _httpContextAccessor.HttpContext.Items["TempIdCounter"] = counter;

                    return counter;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ID de registro para auditoría");
                return 0;
            }
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}