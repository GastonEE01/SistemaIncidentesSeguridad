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
    [Authorize]
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
        [Authorize]
        public async Task<IActionResult> CrearTicket()
        {
            var categorias = await _categoriaLogica.ObtenerCategorias();
            var prioridades = await _prioridadLogica.ObtenerPrioridades();

            ViewBag.Categorias = categorias;
            ViewBag.Prioridades = prioridades;

            return View(new TicketModel()); 
        }

        [HttpPost]
        [Authorize]
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

            var ticket = new Ticket
            {
                Titulo = ticketModel.Titulo,
                Descripcion = ticketModel.Descripcion,
                IdCategoria = ticketModel.IdCategoria,
                IdPrioridad = ticketModel.IdPrioridad,
                IdEstado = 1, 
                FechaCreacion = DateTime.Now,
                IdUsuario = _usuarioLogica.ObtenerIdUsuario(User) ?? throw new InvalidOperationException("Usuario no autenticado")
            };

            await _tiketLogica.CrearTicket(ticket);
            TempData["SuccessMessage"] = "Ticket creado correctamente.";
            return RedirectToAction("Index","Home");
        }
         

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
