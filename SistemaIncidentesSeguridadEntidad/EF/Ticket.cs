using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaIncidentesSeguridad.EF;

public partial class Ticket
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public int IdUsuario { get; set; }

    public int IdCategoria { get; set; }

    public int IdEstado { get; set; }

    public int IdPrioridad { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Prioridad IdPrioridadNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
