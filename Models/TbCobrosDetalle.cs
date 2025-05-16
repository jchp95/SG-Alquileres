namespace Alquileres.Models;

public partial class TbCobrosDetalle
{
    public int Fid { get; set; }

    public int FkidCobro { get; set; }

    public int FnumeroCuota { get; set; }

    public decimal Fmonto { get; set; }

    public decimal Fmora { get; set; }

    public bool Factivo { get; set; }
}
