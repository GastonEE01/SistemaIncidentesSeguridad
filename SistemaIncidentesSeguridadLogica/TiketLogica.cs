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
        Task ActualizarEstado(int id, int nuevoEstado);

        Task<List<Estado>> ObtenerEstados();

        Task<List<Ticket>> ObtenerTikectPendiente();
        Task<Ticket> ObtenerTikectPorId(int idTikect);
        Task<List<Ticket>>ObtenerTikectRespondidos();
        Task<List<Ticket>> ObtenerUsuario();

    }

    public class TiketLogica : ITiketLogica
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        public TiketLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> ObtenerTikectPendiente()
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Include(t => t.Comentarios) 
                .Where(t => !t.Comentarios.Any())
                .ToListAsync();
        }

        public async Task<Ticket> ObtenerTikectPorId(int idTikect)
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .FirstOrDefaultAsync(t => t.Id == idTikect); // 👈 SOLO UNO
        }

        public async Task ActualizarEstado(int id, int nuevoEstado)
        {
            var ticket = _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
            {
                throw new ArgumentException("El ticket no existe.");
            }
            ticket.Result.IdEstado = nuevoEstado;
            await _context.SaveChangesAsync();

        }

        public async Task<List<Estado>> ObtenerEstados()
        {
            return await _context.Estados.ToListAsync();
        }

        public async Task<List<Ticket>> ObtenerUsuario()
        {
            return null;
         //   return await _context.Tickets.Where(t => t.UsuarioId == 1).ToListAsync(); // Cambia el 1 por el ID del usuario que deseas filtrar
        }

        public async Task<List<Ticket>> ObtenerTikectRespondidos()
        {
            return await _context.Tickets
                          .Include(t => t.IdUsuarioNavigation)
                          .Include(t => t.IdEstadoNavigation)
                          .Include(t => t.IdCategoriaNavigation)
                          .Include(t => t.IdPrioridadNavigation)
                          .Include(t => t.Comentarios)
                          .Where(t => t.Comentarios.Any())
                          .ToListAsync();
           
        }

        

      
    }
}
