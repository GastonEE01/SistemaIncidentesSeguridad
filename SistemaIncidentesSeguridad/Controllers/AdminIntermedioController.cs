using Microsoft.AspNetCore.Mvc;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AdminIntermedioController : Controller
    {
        private readonly ITiketLogica _tiketLogica;

        public AdminIntermedioController(ITiketLogica tiketLogica)
        {
            _tiketLogica = tiketLogica;
        }

        public async Task<IActionResult> Index()
        {
            var tikets =  await _tiketLogica.ObtenerTikect();
            return View(tikets);
        }
    }
}
