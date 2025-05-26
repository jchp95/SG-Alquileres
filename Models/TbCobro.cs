using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Alquileres.Enums;

namespace Alquileres.Models
{
    public partial class TbCobro
    {
        public int FidCobro { get; set; }

        public int FkidCxc { get; set; }

        [DisplayName("Fecha")]
        public DateOnly Ffecha { get; set; }

        [DisplayName("Hora")]
        public TimeOnly Fhora { get; set; }

        [Required]
        [DisplayName("Monto")]
        public decimal Fmonto { get; set; }

        [DisplayName("Descuento")]
        public decimal Fdescuento { get; set; }

        [DisplayName("Cargos")]
        public decimal Fcargos { get; set; }

        [DisplayName("Concepto")]
        public string Fconcepto { get; set; } = null!;

        [DisplayName("Motivo de anulación")]
        public string FmotivoAnulacion { get; set; } = string.Empty;

        [DisplayName("Fecha de anulación")]
        public DateOnly? FfechaAnulacion { get; set; }

        [DisplayName("Usuario")]
        public int FkidUsuario { get; set; }

        [DisplayName("Origen")]
        public int FkidOrigen { get; set; }

        [NotMapped]
        public OrigenCobro Origen
        {
            get => (OrigenCobro)FkidOrigen;
            set => FkidOrigen = (int)value;
        }

        [NotMapped] // Indica que no es una columna de la base de datos
        public DateTime FechaCompleta => Ffecha.ToDateTime(Fhora);

        [DisplayName("Activo")]
        public bool Factivo { get; set; }

    }
}
