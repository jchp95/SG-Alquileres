using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models

{
    [Table("tb_moneda")]
    public partial class TbMoneda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_moneda")]
        public int FidMoneda { get; set; }

        [Column("fmoneda", TypeName = "varchar(10)")]
        public string Fmoneda { get; set; } = null!;

        [Column("fsimbolo", TypeName = "varchar(5)")]
        public string Fsimbolo { get; set; } = null!;

        [Column("factivo")]
        public bool Factivo { get; set; }
    }
}
