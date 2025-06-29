using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    [Authorize(Roles = "3")]
    public class AdminGeneralController : Controller
    {
        private readonly ITiketLogica _tiketLogica;
        private readonly IUsuarioLogica _usuarioLogica;
        private readonly ILogger<AdminGeneralController> _logger;

        public AdminGeneralController(ITiketLogica ticketLogica, IUsuarioLogica usuarioLogica, ILogger<AdminGeneralController> logger)
        {
           _tiketLogica = ticketLogica;
           _logger = logger;
           _usuarioLogica = usuarioLogica;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _tiketLogica.ObtenerUsuariosConCantidadTickets();
            var tickets = await _tiketLogica.ObtenerResumenTickets();

            ViewBag.Usuarios = usuarios;
            ViewBag.Tickets = tickets;
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> VerificarTicketsEnProgreso(int id)
        {
            int ticketsEnProgreso = await _tiketLogica.ContarTicketsEnProgresoPorUsuario(id);
            if (ticketsEnProgreso > 0)
            {
                TempData["Mensaje"] = $"El usuario tiene {ticketsEnProgreso} tickets en progreso.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int id, bool confirmarEliminacion = false)
        {
            try
            {
                var idUsuarioLogueado = _usuarioLogica.ObtenerIdUsuario(User);

                if (id == idUsuarioLogueado)
                {
                    TempData["ErrorMessage"] = "No podés eliminarte a vos mismo.";
                    return RedirectToAction(nameof(Index));
                }

                var resultado = await _tiketLogica.EliminarUsuarioAsync(id, confirmarEliminacion);

                if (!resultado.Exito)
                {
                    if (resultado.NecesitaConfirmacion)
                    {
                        TempData["ErrorMessage"] = resultado.Mensaje;
                        TempData["UsuarioId"] = id;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = resultado.Mensaje;
                    }
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = resultado.Mensaje;
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
            bool eliminado = await _tiketLogica.EliminarTicketAsync(id);

            if (!eliminado)
            {
                TempData["ErrorMessage"] = "No se encontró el ticket especificado.";
            }
            else
            {
                TempData["SuccessMessage"] = "Ticket eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
