namespace Alquileres.Models
{
    public class DashboardViewModel
    {
        public int InmueblesActivosCount { get; set; }
        public int InmueblesActivosMesPasado { get; set; }
        public double? PorcentajeCambioInmuebles { get; set; }
        public string TextoCambioInmuebles { get; set; }
        public string ClaseCambioInmuebles { get; set; }
        public int CuotasVencidasCount { get; set; }
        public List<TbInmueble> InmueblesActivos { get; set; }

        // Nuevas propiedades para ocupación
        public double OcupacionActual { get; set; }
        public double OcupacionMesPasado { get; set; }
        public int TotalInmuebles { get; set; }

        // Nuevas propiedades para ingresos
        public decimal IngresosMensuales { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal? CambioIngresos { get; set; }
        public string TextoCambioIngresos { get; set; }
        public string ClaseCambioIngresos { get; set; }

        public List<ActividadReciente> ActividadesRecientes { get; set; }


    }
}