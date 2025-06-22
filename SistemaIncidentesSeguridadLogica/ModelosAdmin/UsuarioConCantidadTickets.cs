using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica.ModelosAdmin
{
    public class UsuarioConCantidadTickets
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public int Rol { get; set; }
        public int TicketsCreados { get; set; }
    }
}
