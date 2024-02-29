using System;
using System.Collections.Generic;

namespace APIFC3.Data.Models;

public partial class Usuario
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();
}
