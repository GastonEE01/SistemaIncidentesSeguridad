using System;
using System.Collections.Generic;

namespace SistemaIncidentesSeguridad.EF;

public partial class Categorium
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
