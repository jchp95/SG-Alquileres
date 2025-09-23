using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models

{
    public partial class GastoViewModel
    {

        public int FidGasto { get; set; }
        public int FkidGastoTipo { get; set; }

        public decimal Fmonto { get; set; }

        public string Fdescripcion { get; set; }

        public DateOnly Ffecha { get; set; }

        public string TipoGasto { get; set; }
        public string NombreUsuario { get; set; }

        public bool Factivo { get; set; }

        public IEnumerable<TbGastoTipo> TiposGasto { get; set; }
    }
}
