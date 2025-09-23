using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models;

[Table("tb_cxc")]
public partial class TbCxc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fid_cuenta")]
    public int FidCuenta { get; set; }

    [ForeignKey("FidInquilino")]
    [Column("fkid_inquilino")]
    [Display(Name = "Inquilino")]
    public int? FkidInquilino { get; set; }

    [ForeignKey("FidInmueble")]
    [Column("fkid_inmueble")]
    [Display(Name = "Inmueble")]
    public int? FkidInmueble { get; set; }
    [ForeignKey("FidPeridoPago")]
    [Column("fid_periodo_pago")]
    [Display(Name = "Período de Pago")]
    public int FkidPeriodoPago { get; set; }

    [ForeignKey("FidUsuario")]
    [Column("fkid_usuario")]
    public int FkidUsuario { get; set; }

    [Column("ffecha_inicio", TypeName = "Date")]
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [Display(Name = "Fecha de Inicio")]
    public DateTime FfechaInicio { get; set; }

    [Column("fmonto", TypeName = "decimal(10,2)")]
    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    [Display(Name = "Monto")]
    public decimal Fmonto { get; set; }

    [Column("fdias_gracia")]
    [Display(Name = "Días de Gracia")]
    public int FdiasGracia { get; set; }

    [Column("ftasa_mora", TypeName = "decimal(10,2)")]
    [Display(Name = "Tasa Mora %")]
    public decimal FtasaMora { get; set; }

    [Column("ffecha_prox_cuota", TypeName = "Date")]
    [Display(Name = "Fecha próxima a la Cuota")]
    public DateTime FfechaProxCuota { get; set; }

    [Column("fnota", TypeName = "varchar(250)")]
    [Display(Name = "Notas")]
    public string Fnota { get; set; } = null!;

    [Column("fstatus", TypeName = "varchar(1)")]
    [DisplayName("Estado CxC")]
    public char Fstatus { get; set; }

    [Column("factivo")]
    public bool Factivo { get; set; }

}
