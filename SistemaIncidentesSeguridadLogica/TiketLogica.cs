using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica
{
    public interface ITiketLogica
    {
        Task<List<Ticket>> ObtenerTikect();
        Task<List<Ticket>> ObtenerUsuario();

    }
    public class TiketLogica : ITiketLogica
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        public TiketLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> ObtenerTikect()
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .ToListAsync();
        }

        public async Task<List<Ticket>> ObtenerUsuario()
        {
            return null;
         //   return await _context.Tickets.Where(t => t.UsuarioId == 1).ToListAsync(); // Cambia el 1 por el ID del usuario que deseas filtrar
        }
    }
}
