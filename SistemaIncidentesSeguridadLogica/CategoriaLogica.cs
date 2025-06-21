using Microsoft.EntityFrameworkCore;
using SistemaIncidentesSeguridad.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaIncidentesSeguridadLogica
{
    public interface ICategoriaLogica
    {
        Task<List<Categorium>> ObtenerCategorias();
    }

    public class CategoriaLogica : ICategoriaLogica
    {
        private readonly SistemaGestionDeIncidentesSeguridadContext _context;
        public CategoriaLogica(SistemaGestionDeIncidentesSeguridadContext context)
        {
            _context = context;
        }
        public async Task<List<Categorium>> ObtenerCategorias()
        {
            return await _context.Categoria.ToListAsync();
        }
    }
}
