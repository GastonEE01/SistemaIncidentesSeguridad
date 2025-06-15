using System;
using System.Collections.Generic;

namespace SistemaIncidentesSeguridad.EF;

public partial class Comentario
{
    public int Id { get; set; }

    public int IdTicket { get; set; }

    public int IdUsuario { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
