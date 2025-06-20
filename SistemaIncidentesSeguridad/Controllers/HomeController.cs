using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using SistemaIncidentesSeguridad.Models;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITiketLogica _tiketLogica;
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;

        public HomeController(ILogger<HomeController> logger, ITiketLogica tiketLogica, SistemaGestionDeIncidentesSeguridadContext context)
        {
            _logger = logger;
            _tiketLogica = tiketLogica;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener todos los tickets ordenados por fecha de creación
            var tickets = await _context.Tickets
                .Include(t => t.IdUsuarioNavigation)
                .Include(t => t.IdEstadoNavigation)
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdPrioridadNavigation)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> CrearTicket()
        {
            ViewBag.Categorias = await _context.Categoria.ToListAsync();
            ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearTicket(string titulo, string descripcion, int categoriaId, int prioridadId)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
            {
                ModelState.AddModelError(string.Empty, "Título y descripción son obligatorios.");
                ViewBag.Categorias = await _context.Categoria.ToListAsync();
                ViewBag.Prioridades = await _context.Prioridads.ToListAsync();
                return View();
            }

            // Obtener el primer usuario de la base de datos (hardcodeado como mencionaste)
            var usuario = await _context.Usuarios.FirstOrDefaultAsync();
            if (usuario == null)
            {
                // Crear un usuario por defecto si no existe
                usuario = new Usuario
                {
                    Nombre = "Usuario",
                    Apellido = "Demo",
                    CorreoElectronico = "usuario@demo.com",
                    Contrasenia = "demo123",
                    Rol = 1 // Usuario normal
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            var nuevoTicket = new Ticket
            {
                Titulo = titulo,
                Descripcion = descripcion,
                FechaCreacion = DateTime.Now,
                IdUsuario = usuario.Id,
                IdCategoria = categoriaId,
                IdEstado = 1, // Estado "Abierto" (ID = 1)
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
