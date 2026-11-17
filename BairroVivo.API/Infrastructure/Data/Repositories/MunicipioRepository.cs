using BairroVivo.API.Domain.Interfaces;
using BairroVivo.Domain.Entities;

namespace BairroVivo.Infrastructure.Data.Repositories
{
    public class MunicipioRepository : IMunicipioRepository
    {
        private readonly ApplicationContext _context;

        public MunicipioRepository(ApplicationContext context)
        {
            _context = context;
        }

        public IEnumerable<MunicipioEntity> ObterTodos(string? nome = null, string? uf = null)
        {
            var consulta = _context.Municipios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var filtroNome = nome.Trim().ToUpperInvariant();
                consulta = consulta.Where(m => m.Nome.ToUpper().Contains(filtroNome));
            }

            if (!string.IsNullOrWhiteSpace(uf))
            {
                var filtroUf = uf.Trim().ToUpperInvariant();
                consulta = consulta.Where(m => m.Uf.ToUpper() == filtroUf);
            }

            return consulta.OrderBy(m => m.Nome).ToList();
        }

        public MunicipioEntity? ObterPorId(long idMunicipio) =>
            _context.Municipios.FirstOrDefault(m => m.IdMunicipio == idMunicipio);
    }
}
