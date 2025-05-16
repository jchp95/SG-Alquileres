using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models
{
    public class TbPeriodoPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Días")]
        public int Dias { get; set; }
    }
}