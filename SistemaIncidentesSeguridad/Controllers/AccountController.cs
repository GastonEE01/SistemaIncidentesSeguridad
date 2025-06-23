using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
                usuario.Rol = 3; 
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
        public async Task<IActionResult> IniciarSesion(Login LoginModel)
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

                var token = JwtHelper.GenerarToken(usuarioCredenciales, _config);
                
                HttpContext.Session.SetString("JWT", token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, $"{usuarioCredenciales.Nombre} {usuarioCredenciales.Apellido}"),
                    new Claim(ClaimTypes.Email, usuarioCredenciales.CorreoElectronico),
                    new Claim(ClaimTypes.Role, usuarioCredenciales.Rol.ToString()),
                    new Claim("UserId", usuarioCredenciales.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                HttpContext.Session.SetString("UserEmail", usuarioCredenciales.CorreoElectronico);
                HttpContext.Session.SetString("UserName", $"{usuarioCredenciales.Nombre} {usuarioCredenciales.Apellido}");
                HttpContext.Session.SetInt32("UserRole", usuarioCredenciales.Rol);
                HttpContext.Session.SetInt32("UserId", usuarioCredenciales.Id);

                TempData["SuccessMessage"] = $"¡Bienvenido, {usuarioCredenciales.Nombre}!";

                return usuarioCredenciales.Rol switch
                {
                    3 => RedirectToAction("Index", "AdminGeneral"),
                    2 => RedirectToAction("Index", "AdminIntermedio"),
                    _ => RedirectToAction("Index", "Home")
                };

            }
            catch (UsuarioNoEncontradoException)
            {
                ModelState.AddModelError(string.Empty, "El usuario no existe.");
                return View("Login", LoginModel);
            }
            catch (ContrasenaIncorrectaException)
            {
                ModelState.AddModelError(string.Empty, "La contraseña es incorrecta.");
                return View("Login", LoginModel);
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
            try
            {
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!result.Succeeded)
                {
                    TempData["Error"] = "Autenticación con Google falló.";
                    return RedirectToAction("Login", "Account");
                }

                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "No se pudo obtener el correo electrónico de Google.";
                    return RedirectToAction("Login", "Account");
                }

                var usuario = _usuarioLogica.BuscarUsuario(email);

                if (usuario == null)
                {
                    email = email.ToLower(); 
                    int rol;

                    if (email == "admingeneral@gmail.com")
                        rol = 1;
                    else if (email == "adminintermedio@gmail.com")
                        rol = 2;
                    else
                        rol = 3; 

                    var nuevoUsuario = new Usuario
                    {
                        Nombre = name?.Split(' ').FirstOrDefault() ?? "Usuario",
                        Apellido = name?.Split(' ').Skip(1).FirstOrDefault() ?? "Google",
                        CorreoElectronico = email,
                        Contraseña = Guid.NewGuid().ToString(),
                        Rol = rol
                    };

                    _usuarioLogica.CrearUsuario(nuevoUsuario);
                    usuario = _usuarioLogica.BuscarUsuario(email);
                }

                if (usuario == null)
                {
                    TempData["Error"] = "Error al crear/obtener el usuario.";
                    return RedirectToAction("Login", "Account");
                }

                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                    new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                    new Claim("UserId", usuario.Id.ToString())
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                var token = JwtHelper.GenerarToken(usuario, _config);
                HttpContext.Session.SetString("JWT", token);

                HttpContext.Session.SetString("UserEmail", usuario.CorreoElectronico);
                HttpContext.Session.SetString("UserName", $"{usuario.Nombre} {usuario.Apellido}");
                HttpContext.Session.SetInt32("UserRole", usuario.Rol);
                HttpContext.Session.SetString("UserId", usuario.Id.ToString());

                TempData["SuccessMessage"] = $"¡Bienvenido, {usuario.Nombre}!";

                var emailLower = usuario.CorreoElectronico.ToLower();
                if (emailLower == "admingeneral@gmail.com")
                {
                    return RedirectToAction("Index", "AdminGeneral");
                }
                else if (emailLower == "adminintermedio@gmail.com")
                {
                    return RedirectToAction("Index", "AdminIntermedio");
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GoogleResponse: {Message}", ex.Message);
                TempData["Error"] = "Error durante la autenticación con Google.";
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetToken()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("No hay token JWT válido en la sesión");
            }
            
            return Ok(new { token = token });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
