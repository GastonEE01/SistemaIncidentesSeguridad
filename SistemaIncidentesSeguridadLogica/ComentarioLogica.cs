using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica
{
    public interface IComentarioLogica
    {
        Task AgregarComentario(Comentario comentario);
        Task<DateTime?> ObtenerFechaUltimaRespuesta(int ticketId);
        Task<Comentario?> ObtenerUltimoComentario(int ticketId);

    }

    public class ComentarioLogica : IComentarioLogica
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;

        public ComentarioLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }

        public async Task AgregarComentario(Comentario comentario)
        {
            if (comentario == null)
            {
                throw new ArgumentNullException(nameof(comentario), "El comentario no puede ser nulo.");
            }

            if (string.IsNullOrWhiteSpace(comentario.Contenido))
            {
                throw new ArgumentException("El contenido del comentario no puede estar vacío.");
            }

            // Validar que el ticket existe
            var ticketExiste = await _context.Tickets.AnyAsync(t => t.Id == comentario.IdTicket);
            if (!ticketExiste)
            {
                throw new InvalidOperationException($"El ticket con ID {comentario.IdTicket} no existe en la base de datos.");
            }

            // Validar que el usuario existe
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == comentario.IdUsuario);
            if (!usuarioExiste)
            {
                throw new InvalidOperationException($"El usuario con ID {comentario.IdUsuario} no existe en la base de datos.");
            }

            comentario.Fecha = DateTime.Now;
            comentario.Contenido = comentario.Contenido.Trim();

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task<DateTime?> ObtenerFechaUltimaRespuesta(int ticketId)
        {
            return await _context.Comentarios
                .Where(c => c.IdTicket == ticketId)
                .OrderByDescending(c => c.Fecha)
                .Select(c => (DateTime?)c.Fecha)
                .FirstOrDefaultAsync(); // devuelve null si no hay comentarios
        }

        public async Task<Comentario?> ObtenerUltimoComentario(int ticketId)
        {
            return await _context.Comentarios
                .Where(c => c.IdTicket == ticketId)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefaultAsync();
        }
    }
}
