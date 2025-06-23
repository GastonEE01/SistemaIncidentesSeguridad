using Microsoft.AspNetCore.Mvc;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
