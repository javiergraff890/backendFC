using System;
using System.Collections.Generic;

namespace APIFC3.Data.Models;

public partial class Movimiento
{
    public int Id { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Valor { get; set; }

    public int IdCaja { get; set; }

    public virtual Caja IdCajaNavigation { get; set; } = null!;
}
