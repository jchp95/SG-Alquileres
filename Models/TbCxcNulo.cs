using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Alquileres.Enums;

namespace Alquileres.Models
{
    [Table("tb_cxc_nulo")]
    public partial class TbCxcNulo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_cxc_nulo")]
        public int FidCuentaNulo { get; set; }

        [ForeignKey("FidCuenta")]
        [Column("fkid_cuenta")]
        public int FkidCuenta { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("fhora")]
        [DisplayName("Hora")]
        public TimeOnly Fhora { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

        [Column("fmotivo_anulacion", TypeName = "varchar(250)")]
        [DisplayName("Motivo de anulación")]
        public string FmotivoAnulacion { get; set; } = string.Empty;

        [Column("ffecha_anulacion", TypeName = "Date")]
        [DisplayName("Fecha de anulación")]
        public DateOnly? FfechaAnulacion { get; set; }

    }
}
