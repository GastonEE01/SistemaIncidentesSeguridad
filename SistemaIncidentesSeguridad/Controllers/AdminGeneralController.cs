using Microsoft.AspNetCore.Mvc;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AdminGeneralController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
