using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models

{
    [Table("tb_inquilino")]
    public partial class TbInquilino
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_inquilino")]
        public int FidInquilino { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("fnombre", TypeName = "varchar(50)")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        [DisplayName("Nombre")]
        public string Fnombre { get; set; } = null!;

        [Column("fapellidos", TypeName = "varchar(100)")]
        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [Display(Name = "Apellidos")]
        public string Fapellidos { get; set; } = null!;

        [Column("fcedula", TypeName = "varchar(50)")]
        [Required(ErrorMessage = "La cédula es obligatoria")]
        [Display(Name = "Cédula")]
        public string Fcedula { get; set; } = null!;

        [Column("fdireccion", TypeName = "varchar(200)")]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede tener más de 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Fdireccion { get; set; } = null!;

        [Column("ftelefono", TypeName = "varchar(50)")]
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        public string Ftelefono { get; set; } = null!;

        [Column("fcelular", TypeName = "varchar(50)")]
        [Required(ErrorMessage = "El celular es obligatorio")]
        [Display(Name = "Celular")]
        public string Fcelular { get; set; } = null!;

        [Column("ffecha_registro", TypeName = "Date")]
        public DateTime FfechaRegistro { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }

    }
}
