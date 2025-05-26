namespace Alquileres.Models
{
    public class ActividadReciente
    {
        public string Tipo { get; set; } // "Inquilino", "Propietario", "Inmueble"
        public string Descripcion { get; set; } // Ej: "Juan Pérez registrado como nuevo inquilino"
        public DateTime Fecha { get; set; } // Fecha y hora del evento
        public string DetalleAdicional { get; set; }
        public bool EsUrgente { get; set; }

    }

}