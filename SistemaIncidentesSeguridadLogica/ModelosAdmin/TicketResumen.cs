using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica.ModelosAdmin
{
    public class TicketResumen
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Prioridad { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
    }
}
