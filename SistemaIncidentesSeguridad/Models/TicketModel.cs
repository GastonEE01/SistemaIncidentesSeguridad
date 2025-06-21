using System.ComponentModel.DataAnnotations;
using SistemaIncidentesSeguridad.EF;

namespace SistemaIncidentesSeguridad.Models
{
    public class TicketModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "La prioridad es obligatoria")]
        public int IdPrioridad { get; set; }

        public string Estado { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int CantidadComentarios { get; set; }

        // Para mostrar la fecha ticket respondido
        public Ticket? Ticket { get; set; }
        public DateTime? FechaUltimaRespuesta { get; set; }
        public string? UltimoComentario { get; set; }
    }
}

