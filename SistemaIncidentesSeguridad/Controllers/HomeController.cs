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
        private readonly ILogger<HomeController> _logger;
        private readonly ITiketLogica _tiketLogica;
        private readonly IComentarioLogica _comentarioLogica;

        private readonly SistemaGestionDeIncidentesSeguridadContext _context;

        public HomeController(ILogger<HomeController> logger, ITiketLogica tiketLogica,IComentarioLogica comentarioLofica, SistemaGestionDeIncidentesSeguridadContext context)
        {
            _logger = logger;
            _tiketLogica = tiketLogica;
            _context = context;
            _comentarioLogica = comentarioLofica;

        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var tickets = await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .Where(t => t.IdUsuario.ToString() == userId) 
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            var ticketModels = new List<TicketModel>();
            var respondidos = await _tiketLogica.ObtenerTikectRespondidos();

            foreach (var ticket in tickets)
            {
                var ultimoComentario = await _comentarioLogica.ObtenerUltimoComentario(ticket.Id);

                ticketModels.Add(new TicketModel
                {
                    Ticket = ticket,
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
            ViewBag.Categorias = await _context.Categoria.ToListAsync();
            ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTicket(string titulo, string descripcion, int categoriaId, int prioridadId)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
            {
                ViewBag.Categorias = await _context.Categoria.ToListAsync();
                ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
                ModelState.AddModelError(string.Empty, "Título y descripción son obligatorios.");
                return View();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userId, out int usuarioId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario autenticado. Por favor, inicie sesión nuevamente.");
                ViewBag.Categorias = await _context.Categoria.ToListAsync();
                ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
                return View();
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró el usuario en el sistema. Por favor, inicie sesión nuevamente.");
                ViewBag.Categorias = await _context.Categoria.ToListAsync();
                ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
                return View();
            }

            var nuevoTicket = new Ticket
            {
                Titulo = titulo,
                Descripcion = descripcion,
                FechaCreacion = DateTime.Now,
                IdUsuario = usuarioId,
                IdCategoria = categoriaId,
                IdEstado = 1, 
                IdPrioridad = prioridadId
            };

            _context.Tickets.Add(nuevoTicket);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ticket creado exitosamente.";
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
