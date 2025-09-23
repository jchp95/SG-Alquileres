using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_empresa")]
    public class Empresa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_empresa")]
        public int IdEmpresa { get; set; }

        [Column("frnc", TypeName = "varchar(18)")]
        public string? Rnc { get; set; }

        [Column("fnombre", TypeName = "varchar(250)")]
        public string? Nombre { get; set; }

        [Column("fdireccion", TypeName = "varchar(250)")]
        public string? Direccion { get; set; }

        [Column("ftelefonos", TypeName = "varchar(14)")]
        public string? Telefonos { get; set; }

        [Column("festlogan", TypeName = "varchar(250)")]
        public string? Slogan { get; set; }

        [Column("fmensaje", TypeName = "varchar(250)")]
        public string? Mensaje { get; set; }

        [Column("flogo")]
        public byte[]? Logo { get; set; }

        [Column("ffondo")]
        public byte[]? Fondo { get; set; }

        [Column("fcodigoqr_web")]
        public byte[]? CodigoQrWeb { get; set; }

        [Column("fcodigoqr_redes")]
        public byte[]? CodigoQrRedes { get; set; }

        [Column("femail", TypeName = "varchar(50)")]
        public string? Email { get; set; }

        [Column("fcontraseña", TypeName = "varchar(100)")]
        public string? Contrasena { get; set; }

        [Column("factivar_cobro_rapido")]
        public bool ActivarCobroRapido { get; set; } = true;

        [NotMapped]
        public int TipoComprobantePorDefecto { get; set; }
       
    }
}
