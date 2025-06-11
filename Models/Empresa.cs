using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    public class Empresa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_empresa")]
        public int IdEmpresa { get; set; } // Este campo no puede ser NULL, ya que es la clave primaria.

        [Column("frnc")]
        [StringLength(18)]
        public string? Rnc { get; set; } // Nullable, ya que puede ser NULL en la base de datos.

        [Column("fnombre")]
        [StringLength(250)]
        public string? Nombre { get; set; } // Nullable.

        [Column("fdireccion")]
        [StringLength(250)]
        public string? Direccion { get; set; } // Nullable.

        [Column("ftelefonos")]
        [StringLength(14)]
        public string? Telefonos { get; set; } // Nullable.

        [Column("festlogan")]
        [StringLength(250)]
        public string? Slogan { get; set; } // Nullable.

        [Column("fmensaje")]
        [StringLength(250)]
        public string? Mensaje { get; set; } // Nullable.

        [Column("flogo")]
        public byte[]? Logo { get; set; } // Nullable.

        [Column("ffondo")]
        public byte[]? Fondo { get; set; } // Nullable.

        [Column("fcodigoqr_web")]
        public byte[]? CodigoQrWeb { get; set; } // Nullable.

        [Column("fcodigoqr_redes")]
        public byte[]? CodigoQrRedes { get; set; } // Nullable.

        [Column("femail")]
        [StringLength(50)]
        public string? Email { get; set; } // Nullable.

        [Column("fcontraseña")]
        [StringLength(100)]
        public string? Contrasena { get; set; } // Nullable.
    }
}
