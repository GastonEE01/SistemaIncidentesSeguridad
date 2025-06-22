using System;
using System.Collections.Generic;

namespace SistemaIncidentesSeguridad.EF;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string CorreoElectronico { get; set; } = null!;

    public string Contrasenia { get; set; } = null!;

    public int Rol { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
