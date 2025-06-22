using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica
{
    public interface IPrioridadLogica 
    {
        Task<List<Prioridad>> ObtenerPrioridades();
    }
    public class PrioridadLogica : IPrioridadLogica
    {

        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        public PrioridadLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }
        public async Task<List<Prioridad>> ObtenerPrioridades()
        {
            return await _context.Prioridads.ToListAsync();
        }
    }
}
