using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridad.Models;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    [Authorize(Roles = "2,3")] 
    public class AdminIntermedioController : Controller
    {
        private readonly ITiketLogica _tiketLogica;
        private readonly IComentarioLogica _comentarioLogica;


        public AdminIntermedioController(ITiketLogica tiketLogica, IComentarioLogica comentarioLogica)
        {
            _tiketLogica = tiketLogica;
            _comentarioLogica = comentarioLogica;

        }

        [HttpGet]
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

        [HttpGet("pendientes")]
        [Authorize(Roles = "2,3")]
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
        [Authorize(Roles = "2,3")]
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

        [HttpGet("api/tickets/pendientes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetTicketsPendientes()
        {
            try
            {
                var pendientes = await _tiketLogica.ObtenerTikectPendiente();
                return Ok(pendientes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("api/tickets/respondidos")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetTicketsRespondidos()
        {
            try
            {
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

                return Ok(ticketModels);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("api/tickets/{id}/responder")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ResponderTicketApi(int id, [FromBody] ResponderTicketRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ContenidoComentario))
                {
                    return BadRequest(new { error = "Debe ingresar un comentario." });
                }

                await _tiketLogica.ActualizarEstado(id, request.NuevoEstado);

                var comentario = new Comentario
                {
                    IdTicket = id,
                    Contenido = request.ContenidoComentario,
                    IdUsuario = 1
                };

                await _comentarioLogica.AgregarComentario(comentario);
                return Ok(new { message = "Ticket respondido exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ResponderTicketRequest
    {
        public int NuevoEstado { get; set; }
        public string ContenidoComentario { get; set; }
    }
}
