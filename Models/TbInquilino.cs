using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models

{

    public partial class TbInquilino
    {
        public int FidInquilino { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [DisplayName("Nombre")]
        public string Fnombre { get; set; } = null!;

        [Required(ErrorMessage = "Los Apellidos son obligatorios")]
        [Display(Name = "Apellidos")]
        public string Fapellidos { get; set; } = null!;

        [Required(ErrorMessage = "La Cédula es obligatoria")]
        [Display(Name = "Cédula")]
        public string Fcedula { get; set; } = null!;

        [Required(ErrorMessage = "La Dirección es obligatoria")]
        [Display(Name = "Dirección")]
        public string Fdireccion { get; set; } = null!;

        [Required(ErrorMessage = "El Teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        public string Ftelefono { get; set; } = null!;

        [Required(ErrorMessage = "El Celular es obligatorio")]
        [Display(Name = "Celular")]
        public string Fcelular { get; set; } = null!;
        public int FkidUsuario { get; set; }
        public bool Factivo { get; set; }

        public DateTime FfechaRegistro { get; set; }

    }
}
