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
