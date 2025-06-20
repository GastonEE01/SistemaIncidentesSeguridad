using Microsoft.AspNetCore.Mvc;
using SistemaIncidentesSeguridad.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AdminGeneralController : Controller
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        private readonly ILogger<AdminGeneralController> _logger;

        public AdminGeneralController(SistemaGestionDeIncidentesSeguridadContext context, ILogger<AdminGeneralController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.CorreoElectronico,
                    u.Rol,
                    TicketsCreados = u.Tickets.Count
                })
                .ToListAsync();

            var tickets = await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Select(t => new
                {
                    t.Id,
                    t.Titulo,
                    t.Descripcion,
                    t.FechaCreacion,
                    t.FechaResolucion,
                    Usuario = t.IdUsuarioNavigation.Nombre + " " + t.IdUsuarioNavigation.Apellido,
                    Categoria = t.IdCategoriaNavigation.Nombre,
                    Estado = t.IdEstadoNavigation.Nombre,
                    Prioridad = t.IdPrioridadNavigation.Nombre
                })
                .ToListAsync();

            ViewBag.Usuarios = usuarios;
            ViewBag.Tickets = tickets;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerificarTicketsEnProgreso(int id)
        {
            var ticketsEnProgreso = await _context.Tickets
                .Where(t => t.IdUsuario == id && t.IdEstado == 2) // Estado 2 = En Progreso
                .CountAsync();

            return Json(new { tieneTicketsEnProgreso = ticketsEnProgreso > 0, cantidadTickets = ticketsEnProgreso });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int id, bool confirmarEliminacion = false)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.Tickets)
                        .ThenInclude(t => t.Comentarios)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el usuario especificado.";
                    return RedirectToAction(nameof(Index));
                }

                // Solo proteger la cuenta de admingeneral@gmail.com
                if (usuario.CorreoElectronico.ToLower() == "admingeneral@gmail.com")
                {
                    TempData["ErrorMessage"] = "No se puede eliminar la cuenta de administrador general.";
                    return RedirectToAction(nameof(Index));
                }

                if (!confirmarEliminacion)
                {
                    var ticketsEnProgreso = usuario.Tickets.Count(t => t.IdEstado == 2);
                    if (ticketsEnProgreso > 0)
                    {
                        TempData["ErrorMessage"] = $"El usuario tiene {ticketsEnProgreso} ticket(s) en progreso. ¿Está seguro que desea eliminarlo?";
                        TempData["UsuarioId"] = id;
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Primero eliminamos todos los comentarios de todos los tickets del usuario
                foreach (var ticket in usuario.Tickets)
                {
                    var comentariosTicket = await _context.Comentarios
                        .Where(c => c.IdTicket == ticket.Id)
                        .ToListAsync();
                    _context.Comentarios.RemoveRange(comentariosTicket);
                }
                await _context.SaveChangesAsync();

                // Luego eliminamos todos los comentarios hechos por el usuario en otros tickets
                var comentariosUsuario = await _context.Comentarios
                    .Where(c => c.IdUsuario == id)
                    .ToListAsync();
                _context.Comentarios.RemoveRange(comentariosUsuario);
                await _context.SaveChangesAsync();

                // Ahora eliminamos todos los tickets del usuario
                _context.Tickets.RemoveRange(usuario.Tickets);
                await _context.SaveChangesAsync();

                // Finalmente eliminamos el usuario
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "No hay detalles adicionales.";
                _logger.LogError(ex, "Error de base de datos al eliminar usuario {UserId}. Detalles: {Details}", id, innerException);
                TempData["ErrorMessage"] = $"Error al eliminar el usuario: {innerException}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {UserId}. Mensaje: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = $"Error al eliminar el usuario: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Eliminar todos los comentarios asociados al ticket
            var comentarios = await _context.Comentarios.Where(c => c.IdTicket == id).ToListAsync();
            _context.Comentarios.RemoveRange(comentarios);

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
