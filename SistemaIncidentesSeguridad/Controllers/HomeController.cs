using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridad.Models;
using SistemaIncidentesSeguridadLogica;
using System.Diagnostics;
using System.Net.Sockets;

namespace SistemaIncidentesSeguridad.Controllers
{
    [Authorize(Roles = "1,2,3")]
    public class HomeController : Controller
    {
        private readonly ITiketLogica _tiketLogica;
        private readonly IComentarioLogica _comentarioLogica;
        private readonly IUsuarioLogica _usuarioLogica;
        private readonly ICategoriaLogica _categoriaLogica;
        private readonly IPrioridadLogica _prioridadLogica;

        public HomeController(IUsuarioLogica usuarioLogica, ITiketLogica tiketLogica,IComentarioLogica comentarioLofica, ICategoriaLogica categoriaLogica, IPrioridadLogica prioridadLogica)
        {
            _usuarioLogica = usuarioLogica;
            _tiketLogica = tiketLogica;
            _comentarioLogica = comentarioLofica;
            _categoriaLogica = categoriaLogica;
            _prioridadLogica = prioridadLogica;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
           var usuarioId = _usuarioLogica.ObtenerIdUsuario(User);

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var tickets = await _tiketLogica.ObtenerTikectDelUsuario(usuarioId.Value);

            var ticketModels = new List<TicketModel>();
            foreach (var t in tickets)
            {
                var ultimoComentario = await _comentarioLogica.ObtenerUltimoComentario(t.Id);

                ticketModels.Add(new TicketModel
                {
                    Ticket = t,
                    FechaCreacion = t.FechaCreacion,
                    UsuarioNombre = t.IdUsuarioNavigation.Nombre,
                    CorreoUsuario = t.IdUsuarioNavigation.CorreoElectronico,
                    Estado = t.IdEstadoNavigation.Nombre,
                    Categoria = t.IdCategoriaNavigation.Nombre,
                    Prioridad = t.IdPrioridadNavigation.Nombre,
                    CantidadComentarios = t.Comentarios.Count,
                    FechaUltimaRespuesta = ultimoComentario?.Fecha,
                    UltimoComentario = ultimoComentario?.Contenido
                });
            }
            return View(ticketModels);

        }

        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> CrearTicket()
        {
            var categorias = await _categoriaLogica.ObtenerCategorias();
            var prioridades = await _prioridadLogica.ObtenerPrioridades();

            ViewBag.Categorias = categorias;
            ViewBag.Prioridades = prioridades;

            return View(new TicketModel()); 
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTicket(TicketModel ticketModel)
        {
            if (!ModelState.IsValid)
            {
                var categorias = await _categoriaLogica.ObtenerCategorias();
                var prioridades = await _prioridadLogica.ObtenerPrioridades();
                ViewBag.Categorias = categorias;
                ViewBag.Prioridades = prioridades;
                return View(ticketModel);
            }

            try
            {
                var usuarioId = _usuarioLogica.ObtenerIdUsuario(User);
                if (usuarioId == null)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario autenticado.");
                    var categorias = await _categoriaLogica.ObtenerCategorias();
                    var prioridades = await _prioridadLogica.ObtenerPrioridades();
                    ViewBag.Categorias = categorias;
                    ViewBag.Prioridades = prioridades;
                    return View(ticketModel);
                }

                var ticket = new Ticket
                {
                    Titulo = ticketModel.Titulo,
                    Descripcion = ticketModel.Descripcion,
                    IdCategoria = ticketModel.IdCategoria,
                    IdPrioridad = ticketModel.IdPrioridad,
                    IdEstado = 1, 
                    FechaCreacion = DateTime.Now,
                    IdUsuario = usuarioId.Value
                };

                await _tiketLogica.CrearTicket(ticket);
                TempData["SuccessMessage"] = "Ticket creado correctamente.";
                return RedirectToAction("Index","Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al crear ticket: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el ticket. Por favor, inténtelo nuevamente.");
                var categorias = await _categoriaLogica.ObtenerCategorias();
                var prioridades = await _prioridadLogica.ObtenerPrioridades();
                ViewBag.Categorias = categorias;
                ViewBag.Prioridades = prioridades;
                return View(ticketModel);
            }
        }

        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> EditarTicket(int id)
        {
            var ticket = await _tiketLogica.ObtenerTikectPorId(id);

            if (ticket == null)
            {
                TempData["ErrorMessage"] = "El ticket solicitado no existe.";
                return RedirectToAction("Index");
            }

            if (ticket.IdEstado != 1)
            {
                TempData["ErrorMessage"] = "El ticket no puede ser editado porque ya está en progreso o cerrado.";
                return RedirectToAction("Index");
            }

            var ticketModel = new EditarTicketModel
            {
                Id = ticket.Id,
                Titulo = ticket.Titulo,
                Descripcion = ticket.Descripcion,
                IdCategoria = ticket.IdCategoria,
                IdPrioridad = ticket.IdPrioridad
            };

            ViewBag.Categorias = await _categoriaLogica.ObtenerCategorias();
            ViewBag.Prioridades = await _prioridadLogica.ObtenerPrioridades();
            return View(ticketModel);
        }


        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> EditarTicket(EditarTicketModel ticketModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var ticket = await _tiketLogica.ObtenerTikectPorId(ticketModel.Id);

                    ticket.Titulo = ticketModel.Titulo;
                    ticket.Descripcion = ticketModel.Descripcion;
                    ticket.IdCategoria = ticketModel.IdCategoria;
                    ticket.IdPrioridad = ticketModel.IdPrioridad;

                    await _tiketLogica.ActualizarTicket(ticket);
                    TempData["SuccessMessage"] = "Ticket actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Categorias = await _categoriaLogica.ObtenerCategorias();
            ViewBag.Prioridades = await _prioridadLogica.ObtenerPrioridades();

            return View(ticketModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
