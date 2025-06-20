namespace SistemaIncidentesSeguridad.Models
{
 
        public class TicketModel
        {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int CantidadComentarios { get; set; }
        }
 }

