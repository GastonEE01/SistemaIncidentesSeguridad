using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaIncidentesSeguridadEntidades;

namespace SistemaIncidentesSeguridadLogica
{

    public interface IRegistroLogica
    {
        void ValidarUsuario(Usuario usuario);
   
    }

    public class RegistroLogica : IRegistroLogica
    {

        public void ValidarUsuario(Usuario usuario)
        {
            if(usuario.Nombre == null || usuario.Apellido == null || usuario.CorreoElectronico == null || usuario.Contraseña == null)
            {
                throw new ArgumentException("Los campos del usuario no pueden ser nulos.");
            }
            if (usuario.Contraseña.Length < 5)
            {
                throw new ArgumentException("La contraseña debe tener al menos 5 caracteres.");
            }
            if (!usuario.Contraseña.Equals(usuario.RepetirContraseña.ToString()))
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }
            if (!usuario.CorreoElectronico.Contains("@"))
            {
                throw new ArgumentException("El correo electrónico debe ser válido.");
            }
            if(!usuario.CorreoElectronico.Contains("gmail.com"))
            {
                throw new ArgumentException("El correo electrónico debe terminar con gmail.com.");
            }
        }
    }
}
