using Alquileres.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_cobro")]
    public partial class TbCobro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_cobro")]
        public int FidCobro { get; set; }

        [ForeignKey("FidCuenta")]
        [Column("fkid_cxc")]
        public int FkidCxc { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [DisplayName("Origen")]
        [Column("fkid_origen")]
        public int FkidOrigen { get; set; }

        [Column("ffecha", TypeName = "Date")]
        [DisplayName("Fecha")]
        public DateOnly Ffecha { get; set; }

        [Column("fhora")]
        [DisplayName("Hora")]
        public TimeOnly Fhora { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

        [Required]
        [Column("fmonto", TypeName = "decimal(10,2)")]
        [DisplayName("Monto")]
        public decimal Fmonto { get; set; }

        [Column("fdescuento", TypeName = "decimal(10,2)")]
        [DisplayName("Descuento")]
        public decimal Fdescuento { get; set; }

        [Column("fcargos", TypeName = "decimal(10,2)")]
        [DisplayName("Cargos")]
        public decimal Fcargos { get; set; }

        [Column("fconcepto", TypeName = "varchar(200)")]
        [DisplayName("Concepto")]
        public string Fconcepto { get; set; } = null!;

        [Column("fncf", TypeName = "varchar(15)")]
        [DisplayName("Número comprobante fiscal")]
        public string Fncf { get; set; } = null!;

        [DisplayName("Fecha vencimiento comprobante fiscal")]
        public DateOnly? FncfVence { get; set; }

        [Column("factivo")]
        [DisplayName("Activo")]
        public bool Factivo { get; set; }

        [NotMapped]
        public OrigenCobro Origen
        {
            get => (OrigenCobro)FkidOrigen;
            set => FkidOrigen = (int)value;
        }

        [NotMapped] // Indica que no es una columna de la base de datos
        public DateTime FechaCompleta => Ffecha.ToDateTime(Fhora);

    }
}
