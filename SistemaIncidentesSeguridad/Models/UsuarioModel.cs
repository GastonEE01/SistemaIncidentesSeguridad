namespace SistemaIncidentesSeguridad.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public string CorreoElectronico { get; set; } = null!;

        public string Contrasenia { get; set; } = null!;

        public int Rol { get; set; }
    }
}
