using Microsoft.AspNetCore.Mvc;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AdminIntermedioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
