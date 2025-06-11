using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Entidades = SistemaIncidentesSeguridadEntidades;
using EF = SistemaIncidentesSeguridad.EF;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SistemaIncidentesSeguridadLogica
{
    public interface IUsuarioLogica
    {
        void CrearUsuario(Entidades.Usuario usuario);
        Entidades.Usuario ValidarCredenciales(string correoElectronico, string contraseña);
        Entidades.Usuario BuscarUsuario(string email);

    }

    public class UsuarioLogica : IUsuarioLogica
    {
        private readonly EF.SistemaGestionDeIncidentesSeguridadContext _context;
        private readonly string _salt;

        public UsuarioLogica(IConfiguration config, EF.SistemaGestionDeIncidentesSeguridadContext context)
        {
            _salt = config["Password:Salt"];
            _context = context;

            if (string.IsNullOrEmpty(_salt))
                throw new InvalidOperationException("Salt no fue configurado correctamente desde appsettings.json.");
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "La contraseña no puede ser nula");

            if (string.IsNullOrEmpty(_salt))
                throw new InvalidOperationException("Salt no configurado correctamente");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        public void CrearUsuario(Entidades.Usuario usuario)
        {
            try
            {
                if (_context.Usuarios.Any(u => u.CorreoElectronico == usuario.CorreoElectronico))
                {
                    throw new InvalidOperationException("El correo electrónico ya está registrado.");
                }

                var usuarioBD = new EF.Usuario
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    CorreoElectronico = usuario.CorreoElectronico,
                    Contrasenia = HashPassword(usuario.Contraseña),
                    Rol = 1
                };

                _context.Usuarios.Add(usuarioBD);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error al crear el usuario en la base de datos.", ex);
            }
        }

        public Entidades.Usuario ValidarCredenciales(string correoElectronico, string contraseña)
        {
            try
            {
                var usuarioBD = _context.Usuarios
                    .FirstOrDefault(u => u.CorreoElectronico.ToLower() == correoElectronico.ToLower());
                var inputHash = HashPassword(contraseña);

                Console.WriteLine("Hash ingresado: " + inputHash);
                Console.WriteLine("Hash base de datos: " + usuarioBD.Contrasenia);

                if (usuarioBD != null && usuarioBD.Contrasenia == HashPassword(contraseña))
                {
                    return new Entidades.Usuario
                    {
                        Id = usuarioBD.Id,
                        Nombre = usuarioBD.Nombre,
                        Apellido = usuarioBD.Apellido,
                        CorreoElectronico = usuarioBD.CorreoElectronico,
                        Contraseña = contraseña,
                        Rol = usuarioBD.Rol
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al validar las credenciales.", ex);
            }

        }

        public Entidades.Usuario BuscarUsuario(string email)
        {
            var usuarioBD = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico.ToLower() == email.ToLower());
            if (usuarioBD == null) return null;

            return new Entidades.Usuario
            {
                Id = usuarioBD.Id,
                Nombre = usuarioBD.Nombre,
                Apellido = usuarioBD.Apellido,
                CorreoElectronico = usuarioBD.CorreoElectronico,
                Rol = usuarioBD.Rol
            };
        }

    }
}

