// ViewModels/CuotaConDatosCxcViewModel.cs

namespace Alquileres.Models
{
    public class CuotaConDatosCxcViewModel
    {
        public TbCxcCuotum Cuota { get; set; }

        public int CantidadCuotas { get; set; }
        public int DiasGracia { get; set; }
        public decimal TasaMora { get; set; }
        public int FkidUsuario { get; set; }

    }
}