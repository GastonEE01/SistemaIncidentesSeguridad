using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadEntidades
{
    public class ResponderTicketRequest
    {
        public int NuevoEstado { get; set; }
        public string ContenidoComentario { get; set; }
    }
}
