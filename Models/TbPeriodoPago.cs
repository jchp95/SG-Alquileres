using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_periodos_pago")]
    public class TbPeriodoPago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_periodo_pago")]
        public int FidPeriodoPago { get; set; }

        [Column("fnombre", TypeName = "varchar(20)")]
        [Display(Name = "Nombre")]
        public string Fnombre { get; set; } = null!;

        [Column("fdias")]
        [Display(Name = "Días")]
        public int Fdias { get; set; }
    }
}