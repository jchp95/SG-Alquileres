namespace Alquileres.Models
{
    public class AuditoriaViewModel
    {
        public string Ftabla { get; set; }
        public int FkidRegistro { get; set; }
        public DateTime Ffecha { get; set; }
        public string Fhora { get; set; }
        public string Faccion { get; set; }
        public string UsuarioNombre { get; set; } // Nombre del usuario
    }
}
