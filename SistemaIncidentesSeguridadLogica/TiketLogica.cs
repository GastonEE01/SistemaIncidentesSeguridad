using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridadLogica.ModelosAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica
{
    public interface ITiketLogica
    {

        Task CrearTicket(Ticket ticket);
        Task ActualizarEstado(int id, int nuevoEstado);
        Task<List<Estado>> ObtenerEstados();
        Task<List<Ticket>> ObtenerTikectPendiente();
        Task<Ticket> ObtenerTikectPorId(int idTikect);
        Task<List<Ticket>> ObtenerTikectRespondidos();
        Task<List<UsuarioConCantidadTickets>> ObtenerUsuariosConCantidadTickets();
        Task<List<TicketResumen>> ObtenerResumenTickets();
        Task<List<Ticket>> ObtenerTodosLosTickets();

        Task<int> ContarTicketsEnProgresoPorUsuario(int usuarioId);
        Task<(bool Exito, string Mensaje, bool NecesitaConfirmacion)> EliminarUsuarioAsync(int id, bool confirmarEliminacion = false);
        Task<bool> EliminarUsuarioAsync(int id);
        Task<bool> EliminarTicketAsync(int id);
        Task<List<Ticket>> ObtenerTikectDelUsuario(int idUsuario);
        Task ActualizarTicket(Ticket ticket);

    }

    public class TiketLogica : ITiketLogica
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        public TiketLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }

        public async Task CrearTicket(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket), "El ticket no puede ser nulo.");
            }

            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == ticket.IdUsuario);
            if (!usuarioExiste)
            {
                throw new InvalidOperationException($"El usuario con ID {ticket.IdUsuario} no existe en la base de datos.");
            }

            var categoriaExiste = await _context.Categoria.AnyAsync(c => c.Id == ticket.IdCategoria);
            if (!categoriaExiste)
            {
                throw new InvalidOperationException($"La categoría con ID {ticket.IdCategoria} no existe en la base de datos.");
            }

            var prioridadExiste = await _context.Prioridades.AnyAsync(p => p.Id == ticket.IdPrioridad);// aca tengo error 
            if (!prioridadExiste)
            {
                throw new InvalidOperationException($"La prioridad con ID {ticket.IdPrioridad} no existe en la base de datos.");
            }

            ticket.FechaCreacion = DateTime.Now;
            ticket.IdEstado = 1;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
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
                .FirstOrDefaultAsync(t => t.Id == idTikect); 
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

        public async Task<List<UsuarioConCantidadTickets>> ObtenerUsuariosConCantidadTickets()
        {
            return await _context.Usuarios
                .Select(u => new UsuarioConCantidadTickets
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    CorreoElectronico = u.CorreoElectronico,
                    Rol = u.Rol,
                    TicketsCreados = u.Tickets.Count
                })
                .ToListAsync();
        }

        public async Task<List<Ticket>> ObtenerTodosLosTickets()
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Include(t => t.Comentarios)
                .ToListAsync();
        }

        public async Task<List<TicketResumen>> ObtenerResumenTickets()
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Select(t => new TicketResumen
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Usuario = t.IdUsuarioNavigation.Nombre + " " + t.IdUsuarioNavigation.Apellido,
                    Categoria = t.IdCategoriaNavigation.Nombre,
                    Estado = t.IdEstadoNavigation.Nombre,
                    Prioridad = t.IdPrioridadNavigation.Nombre,
                    FechaCreacion = t.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<List<Ticket>> ObtenerTotalTicket()
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Include(t => t.Comentarios)  
                .ToListAsync();
        }


        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Tickets)
                .ThenInclude(t => t.Comentarios)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return false;

            if (usuario.CorreoElectronico.ToLower() == "admingeneral@gmail.com")
                throw new InvalidOperationException("No se puede eliminar la cuenta de administrador general.");

            var comentarios = usuario.Tickets.SelectMany(t => t.Comentarios).ToList();
            _context.Comentarios.RemoveRange(comentarios);

            var comentariosUsuario = await _context.Comentarios.Where(c => c.IdUsuario == id).ToListAsync();
            _context.Comentarios.RemoveRange(comentariosUsuario);

            _context.Tickets.RemoveRange(usuario.Tickets);
            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            var comentarios = await _context.Comentarios.Where(c => c.IdTicket == id).ToListAsync();
            _context.Comentarios.RemoveRange(comentarios);

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Ticket>> ObtenerTikectDelUsuario(int idUsuario)
        {
            return await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Include(t => t.Comentarios)
                .Where(t => t.IdUsuario == idUsuario)
                .ToListAsync();
        }

        public async Task<int> ContarTicketsEnProgresoPorUsuario(int usuarioId)
        {
            return await _context.Tickets
                .Where(t => t.IdUsuario == usuarioId && t.IdEstado == 2)
                .CountAsync();
        }

        public async Task<(bool Exito, string Mensaje, bool NecesitaConfirmacion)> EliminarUsuarioAsync(int id, bool confirmarEliminacion = false)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Comentarios)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return (false, "No se encontró el usuario especificado.", false);

            if (usuario.CorreoElectronico.ToLower() == "admingeneral@gmail.com")
                return (false, "No se puede eliminar la cuenta de administrador general.", false);

            if (!confirmarEliminacion)
            {
                int ticketsEnProgreso = usuario.Tickets.Count(t => t.IdEstado == 2);
                if (ticketsEnProgreso > 0)
                    return (false, $"El usuario tiene {ticketsEnProgreso} ticket(s) en progreso. ¿Está seguro que desea eliminarlo?", true);
            }

            var comentariosTickets = usuario.Tickets.SelectMany(t => t.Comentarios).ToList();
            _context.Comentarios.RemoveRange(comentariosTickets);

            var comentariosUsuario = await _context.Comentarios.Where(c => c.IdUsuario == id).ToListAsync();
            _context.Comentarios.RemoveRange(comentariosUsuario);

            _context.Tickets.RemoveRange(usuario.Tickets);

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return (true, "Usuario eliminado correctamente.", false);
        }

        public async Task ActualizarTicket(Ticket ticket)
        {
            var ticketExistente = await _context.Tickets.FindAsync(ticket.Id);

            if (ticketExistente != null)
            {
                if (ticketExistente.IdEstado == 1)
                {
                    ticketExistente.Titulo = ticket.Titulo;
                    ticketExistente.Descripcion = ticket.Descripcion;
                    ticketExistente.IdCategoria = ticket.IdCategoria;
                    ticketExistente.IdPrioridad = ticket.IdPrioridad;

                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException("El ticket no puede ser modificado porque ya está en progreso o cerrado.");
                }
            }
        }


    }
}