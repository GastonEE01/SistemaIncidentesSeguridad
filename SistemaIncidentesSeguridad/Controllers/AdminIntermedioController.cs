using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridad.Models;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AdminIntermedioController : Controller
    {
        private readonly ITiketLogica _tiketLogica;
        private readonly IComentarioLogica _comentarioLogica;


        public AdminIntermedioController(ITiketLogica tiketLogica, IComentarioLogica comentarioLogica)
        {
            _tiketLogica = tiketLogica;
            _comentarioLogica = comentarioLogica;

        }

        public async Task<IActionResult> Index()
        {
            var pendientes = await _tiketLogica.ObtenerTikectPendiente();
            var respondidos = await _tiketLogica.ObtenerTikectRespondidos();
            var ticketModels = new List<TicketModel>();

            foreach (var ticket in respondidos)
            {
                var ultimoComentario = await _comentarioLogica.ObtenerUltimoComentario(ticket.Id);

                ticketModels.Add(new TicketModel
                {
                    Ticket = ticket,
                    FechaUltimaRespuesta = ultimoComentario?.Fecha,
                    UltimoComentario = ultimoComentario?.Contenido
                });
            }

            ViewBag.TicketsRespondidos = ticketModels;
            return View(pendientes);
        }

        /* public async Task<IActionResult> Index()
         {
             var pendientes = await _tiketLogica.ObtenerTikectPendiente();
             var respondidos = await _tiketLogica.ObtenerTikectRespondidos();
             var usuarioModels = new List<TicketModel>();
             foreach (var ticket in respondidos)
             {
                 var fecha = await _comentarioLogica.ObtenerFechaUltimaRespuesta(ticket.Id);
                 usuarioModels.Add(new TicketModel
                 {
                     Ticket = ticket,
                     FechaUltimaRespuesta = fecha
                 });
             }

             ViewBag.TicketsRespondidos = usuarioModels;

             return View(pendientes);
         }*/

        [HttpGet]
        public async Task<IActionResult> ResponderTiket(int id)
        {
            var tiket = await _tiketLogica.ObtenerTikectPorId(id);
            if (tiket == null)
            {
                return NotFound();
            }
            ViewBag.Estados = await _tiketLogica.ObtenerEstados();
            return View("ResponderTiket", tiket);
        }

        [HttpPost]
        public async Task<IActionResult> ResponderTiket(int id, int nuevoEstado, string contenidoComentario)
        {
            if (string.IsNullOrWhiteSpace(contenidoComentario))
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar un comentario.");
                var tiket = await _tiketLogica.ObtenerTikectPorId(id);
                ViewBag.Estados = await _tiketLogica.ObtenerEstados();
                return View("ResponderTiket", tiket);
            }

            await _tiketLogica.ActualizarEstado(id, nuevoEstado);

            var comentario = new Comentario
            {
                IdTicket = id,
                Contenido = contenidoComentario,
                IdUsuario = 1 
            };

            await _comentarioLogica.AgregarComentario(comentario);
            return RedirectToAction("Index");
        }

    }
}
