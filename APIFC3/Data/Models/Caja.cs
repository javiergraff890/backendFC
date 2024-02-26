using System;
using System.Collections.Generic;

namespace APIFC3.Data.Models;

public partial class Caja
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Saldo { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
