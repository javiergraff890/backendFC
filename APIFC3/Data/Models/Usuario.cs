using System;
using System.Collections.Generic;

namespace APIFC3.Data.Models;

public partial class Usuario
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
