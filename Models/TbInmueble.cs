using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_inmueble")]
    public partial class TbInmueble
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_inmueble")]
        public int FidInmueble { get; set; }

        [ForeignKey("FidPropietario")]
        [Column("fkid_propietario")]
        public int FkidPropietario { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [ForeignKey("FidMoneda")]
        [Column("fkid_moneda")]
        [Display(Name = "Tipo de moneda")]
        public int FkidMoneda { get; set; }

        [Column("fdescripcion", TypeName = "varchar(250)")]
        [Display(Name = "Descripción")]
        public string Fdescripcion { get; set; } = null!;

        [Column("fdireccion", TypeName = "varchar(200)")]
        [Display(Name = "Dirección")]
        public string Fdireccion { get; set; } = null!;

        [Column("fubicacion", TypeName = "varchar(100)")]
        [Display(Name = "Ubicación")]
        public string Fubicacion { get; set; } = null!;

        [Column("fprecio", TypeName = "decimal(10,2)")]
        [Display(Name = "Precio")]
        public decimal Fprecio { get; set; }

        [Column("ffecha_registro", TypeName = "Date")]
        public DateTime FfechaRegistro { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }

    }
}


