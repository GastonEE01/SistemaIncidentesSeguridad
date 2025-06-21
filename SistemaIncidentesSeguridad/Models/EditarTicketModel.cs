namespace SistemaIncidentesSeguridad.Models
{
    public class EditarTicketModel
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int IdCategoria { get; set; }

        public int IdPrioridad { get; set; }
    }
}
