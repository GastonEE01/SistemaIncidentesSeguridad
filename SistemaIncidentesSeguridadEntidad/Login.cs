using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadEntidades
{
    public class Login
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
       // [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        //[DataType(DataType.Password)]
        public string Contraseña { get; set; }
        public int Rol { get; set; }

    }
}
