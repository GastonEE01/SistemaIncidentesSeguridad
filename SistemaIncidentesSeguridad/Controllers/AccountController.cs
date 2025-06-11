using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SistemaIncidentesSeguridad.Helper;
using SistemaIncidentesSeguridadEntidades;
using SistemaIncidentesSeguridadLogica;

namespace SistemaIncidentesSeguridad.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioLogica _usuarioLogica;
        private readonly IRegistroLogica _registroLogica;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUsuarioLogica usuarioLogica,
            IRegistroLogica registroLogica,
            IConfiguration config,
            ILogger<AccountController> logger)
        {
            _usuarioLogica = usuarioLogica;
            _registroLogica = registroLogica;
            _config = config;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CrearUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", usuario);
            }

            try
            {
                _registroLogica.ValidarUsuario(usuario);
                _usuarioLogica.CrearUsuario(usuario);
                TempData["SuccessMessage"] = "¡Usuario creado con éxito! Por favor, inicia sesión.";
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Register", usuario);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Register", usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al registrar el usuario.");
                return View("Register", usuario);
            }
        }

        [HttpPost]
        public IActionResult IniciarSesion(Login LoginModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", LoginModel); 
            }

            try
            {
                var usuarioCredenciales = _usuarioLogica.ValidarCredenciales(LoginModel.CorreoElectronico, LoginModel.Contraseña);
                if (usuarioCredenciales == null)
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas. Por favor, intente nuevamente.");
                    return View("Login", LoginModel);
                }

                string token = JwtHelper.GenerarToken(usuarioCredenciales, _config);
                HttpContext.Session.SetString("JWT", token);
                HttpContext.Session.SetString("UserName", $"{usuarioCredenciales.Nombre} {usuarioCredenciales.Apellido}");
                HttpContext.Session.SetInt32("UserRole", usuarioCredenciales.Rol);

                TempData["SuccessMessage"] = $"¡Bienvenido, {usuarioCredenciales.Nombre}!";

                if(usuarioCredenciales.Rol == 1)
                return RedirectToAction("Index", "Home");
                else if (usuarioCredenciales.Rol == 2)
                    return RedirectToAction("Index", "AdminIntermedio");
                else
                {
                    return RedirectToAction("Index", "AdminGeneral");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al validar credenciales: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Error al validar las credenciales. Por favor, intente nuevamente.");
                return View("Login", LoginModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al iniciar sesión: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Por favor, intente más tarde.");
                return View("Login", LoginModel);
            }
        }


        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Autenticación con Google falló.";
                return RedirectToAction("Login", "Account");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var usuario = _usuarioLogica.BuscarUsuario(email);

            if (usuario == null)
            {
                _usuarioLogica.CrearUsuario(new Usuario
                {
                    Nombre = name ?? "Sin nombre",
                    Apellido = "Google",
                    CorreoElectronico = email,
                    Contraseña = Guid.NewGuid().ToString(),
                    Rol = 1
                });

                usuario = _usuarioLogica.BuscarUsuario(email);
            }

            var claimsIdentity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, usuario.Nombre),
        new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
        new Claim(ClaimTypes.Role, usuario.Rol.ToString())
    }, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            TempData["SuccessMessage"] = $"¡Bienvenido, {usuario.Nombre} (Google)!";

            return RedirectToAction("Index", "Home");
        }
    }
}
