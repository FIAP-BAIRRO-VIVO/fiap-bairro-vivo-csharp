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

        public IEnumerable<MunicipioEntity> ObterTodos() =>
            _context.Municipios
                .Where(m => m.Ativo == "S")
                .OrderBy(m => m.Nome)
                .ToList();

        public MunicipioEntity? ObterPorId(decimal idMunicipio) =>
            _context.Municipios.FirstOrDefault(m => m.IdMunicipio == idMunicipio);
    }
}
